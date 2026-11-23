using AutoMapper;
using Datahub.TrainingPortal.Application.DTOs.Assignments;
using Datahub.TrainingPortal.Application.Exceptions;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Common;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Datahub.TrainingPortal.Domain.Enums;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class AssignmentSubmissionService(
    AppDbContext db,
    IMapper mapper,
    ICurrentUserService currentUser,
    IFileStorageService fileStorage) : IAssignmentSubmissionService
{
    private static readonly string[] SubmitterRoles = [RoleNames.Intern, RoleNames.Trainee, RoleNames.Employee];

    public async Task<AssignmentSubmissionDto?> GetMineAsync(Guid trainingId, Guid assignmentId)
    {
        await EnsureAssignmentAsync(trainingId, assignmentId);
        EnsureSubmitterRole();

        var submission = await db.AssignmentSubmissions
            .Include(s => s.Attachments)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.UserId == GetCurrentUserId());

        return submission is null ? null : mapper.Map<AssignmentSubmissionDto>(submission);
    }

    public async Task<IReadOnlyList<AssignmentSubmissionOverviewDto>> GetAllForAssignmentAsync(Guid trainingId, Guid assignmentId)
    {
        await EnsureAssignmentAsync(trainingId, assignmentId);
        var training = await db.Trainings.FirstAsync(t => t.Id == trainingId);

        EnsureManagedDepartment(training.DepartmentId);

        var enrollments = await db.Enrollments
            .Where(e => e.TrainingId == trainingId)
            .Include(e => e.User)
            .ToListAsync();

        var submissionsByUserId = await db.AssignmentSubmissions
            .Where(s => s.AssignmentId == assignmentId)
            .Include(s => s.Attachments)
            .ToDictionaryAsync(s => s.UserId);

        return enrollments
            .Where(e => e.User is not null)
            .Select(e => new AssignmentSubmissionOverviewDto
            {
                UserId = e.UserId,
                UserName = e.User!.FullName,
                Submission = submissionsByUserId.TryGetValue(e.UserId, out var submission)
                    ? mapper.Map<AssignmentSubmissionDto>(submission)
                    : null
            })
            .OrderBy(o => o.UserName)
            .ToList();
    }

    public async Task<AssignmentSubmissionDto> UpsertMineAsync(Guid trainingId, Guid assignmentId, UpsertAssignmentSubmissionRequest request)
    {
        var assignment = await EnsureAssignmentAsync(trainingId, assignmentId);
        EnsureSubmitterRole();
        EnsureOpen(assignment);

        var userId = GetCurrentUserId();
        var now = DateTime.UtcNow;

        var submission = await db.AssignmentSubmissions
            .Include(s => s.Attachments)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.UserId == userId);

        if (submission is null)
        {
            submission = new AssignmentSubmission
            {
                AssignmentId = assignmentId,
                UserId = userId,
                SubmittedAt = now
            };
            db.AssignmentSubmissions.Add(submission);
        }

        submission.TextResponse = request.TextResponse;
        submission.UpdatedAt = now;

        if (request.Attachments is { Count: > 0 })
        {
            foreach (var file in request.Attachments)
            {
                submission.Attachments.Add(await BuildAttachmentAsync(trainingId, assignmentId, userId, file));
            }
        }

        await db.SaveChangesAsync();

        return mapper.Map<AssignmentSubmissionDto>(submission);
    }

    public async Task DeleteAttachmentAsync(Guid trainingId, Guid assignmentId, Guid attachmentId)
    {
        var assignment = await EnsureAssignmentAsync(trainingId, assignmentId);
        EnsureSubmitterRole();
        EnsureOpen(assignment);

        var userId = GetCurrentUserId();
        var submission = await db.AssignmentSubmissions
            .Include(s => s.Attachments)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.UserId == userId)
            ?? throw new NotFoundException("You have not submitted anything for this assignment yet.");

        var attachment = submission.Attachments.FirstOrDefault(a => a.Id == attachmentId)
            ?? throw new NotFoundException($"Attachment '{attachmentId}' was not found.");

        await fileStorage.DeleteAsync(BlobPath(trainingId, assignmentId, userId, attachment.StoredFileName));

        db.AssignmentSubmissionAttachments.Remove(attachment);
        await db.SaveChangesAsync();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAttachmentAsync(Guid trainingId, Guid assignmentId, Guid attachmentId)
    {
        await EnsureAssignmentAsync(trainingId, assignmentId);
        var training = await db.Trainings.FirstAsync(t => t.Id == trainingId);

        var submission = await db.AssignmentSubmissions
            .Include(s => s.Attachments)
            .FirstOrDefaultAsync(s => s.AssignmentId == assignmentId && s.Attachments.Any(a => a.Id == attachmentId))
            ?? throw new NotFoundException($"Attachment '{attachmentId}' was not found.");

        EnsureCanAccessSubmission(submission, training);

        var attachment = submission.Attachments.First(a => a.Id == attachmentId);

        Stream stream;
        try
        {
            stream = await fileStorage.DownloadAsync(BlobPath(trainingId, assignmentId, submission.UserId, attachment.StoredFileName));
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException("The attachment file is missing from storage.");
        }

        return (stream, attachment.ContentType, attachment.FileName);
    }

    private async Task<AssignmentSubmissionAttachment> BuildAttachmentAsync(Guid trainingId, Guid assignmentId, Guid userId, IFormFile file)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("The uploaded file is empty.");
        }

        if (file.Length > AppConstants.MaxFileSizeBytes)
        {
            throw new ArgumentException($"The file exceeds the maximum size of {AppConstants.MaxFileSizeBytes} bytes.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AppConstants.AllowedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"File extension '{extension}' is not allowed.");
        }

        var storedFileName = $"{Guid.NewGuid()}{extension}";

        await using (var stream = file.OpenReadStream())
        {
            await fileStorage.UploadAsync(BlobPath(trainingId, assignmentId, userId, storedFileName), stream, file.ContentType);
        }

        return new AssignmentSubmissionAttachment
        {
            FileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length
        };
    }

    private static string BlobPath(Guid trainingId, Guid assignmentId, Guid userId, string storedFileName)
        => $"{trainingId}/assignments/{assignmentId}/submissions/{userId}/{storedFileName}";

    private async Task<Assignment> EnsureAssignmentAsync(Guid trainingId, Guid assignmentId)
    {
        var assignment = await db.Assignments.FirstOrDefaultAsync(a => a.Id == assignmentId && a.TrainingId == trainingId)
            ?? throw new NotFoundException($"Assignment '{assignmentId}' was not found.");

        return assignment;
    }

    private static void EnsureOpen(Assignment assignment)
    {
        var status = AssignmentStatusCalculator.Compute(assignment.OpensAt, assignment.ClosesAt, DateTime.UtcNow);
        if (status != AssignmentStatus.Open)
        {
            throw new ArgumentException("Submissions are only allowed while the assignment is open.");
        }
    }

    private void EnsureManagedDepartment(Guid departmentId)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin && currentUser.DepartmentId != departmentId)
        {
            throw new ForbiddenException("Department admins can only manage assignments in their own department.");
        }
    }

    private void EnsureSubmitterRole()
    {
        if (!SubmitterRoles.Contains(currentUser.Role))
        {
            throw new ForbiddenException("Only trainees, employees, and interns can submit assignments.");
        }
    }

    private void EnsureCanAccessSubmission(AssignmentSubmission submission, Training training)
    {
        if (submission.UserId == currentUser.UserId)
        {
            return;
        }

        if (currentUser.Role == RoleNames.SuperAdmin)
        {
            return;
        }

        if (currentUser.Role == RoleNames.DepartmentAdmin && currentUser.DepartmentId == training.DepartmentId)
        {
            return;
        }

        throw new ForbiddenException("You do not have access to this submission.");
    }

    private Guid GetCurrentUserId()
        => currentUser.UserId
            ?? throw new UnauthorizedAccessException("A signed-in user is required.");
}
