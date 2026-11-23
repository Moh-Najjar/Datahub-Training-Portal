using AutoMapper;
using Datahub.TrainingPortal.Application.DTOs.Assignments;
using Datahub.TrainingPortal.Application.Exceptions;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Common;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class AssignmentService(
    AppDbContext db,
    IMapper mapper,
    ICurrentUserService currentUser,
    IFileStorageService fileStorage) : IAssignmentService
{
    private static readonly string[] ViewOnlyRoles = [RoleNames.Intern, RoleNames.Trainee, RoleNames.Employee];

    public async Task<IReadOnlyList<AssignmentDto>> GetForTrainingAsync(Guid trainingId)
    {
        await EnsureViewAccessAsync(trainingId);

        var assignments = await db.Assignments
            .Where(a => a.TrainingId == trainingId)
            .Include(a => a.CreatedByUser)
            .Include(a => a.Attachments)
            .OrderBy(a => a.OpensAt)
            .ThenByDescending(a => a.CreatedAt)
            .ToListAsync();

        var now = DateTime.UtcNow;
        return assignments.Select(a => ToDto(a, now)).ToList();
    }

    public async Task<AssignmentDto> CreateAsync(Guid trainingId, CreateAssignmentRequest request)
    {
        var training = await db.Trainings.FirstOrDefaultAsync(t => t.Id == trainingId)
            ?? throw new NotFoundException($"Training '{trainingId}' was not found.");

        EnsureManagedDepartment(training.DepartmentId);

        var assignment = new Assignment
        {
            TrainingId = trainingId,
            Title = request.Title,
            Description = request.Description,
            OpensAt = request.OpensAt,
            ClosesAt = request.ClosesAt,
            CreatedByUserId = GetCurrentUserId()
        };

        db.Assignments.Add(assignment);

        if (request.Attachments is { Count: > 0 })
        {
            foreach (var file in request.Attachments)
            {
                assignment.Attachments.Add(await BuildAttachmentAsync(trainingId, assignment.Id, file));
            }
        }

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(assignment.Id);
    }

    public async Task<AssignmentDto> UpdateAsync(Guid assignmentId, UpdateAssignmentRequest request)
    {
        var assignment = await GetOwnedAssignmentAsync(assignmentId);

        assignment.Title = request.Title;
        assignment.Description = request.Description;
        assignment.OpensAt = request.OpensAt;
        assignment.ClosesAt = request.ClosesAt;
        assignment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(assignment.Id);
    }

    public async Task DeleteAsync(Guid assignmentId)
    {
        var assignment = await GetOwnedAssignmentAsync(assignmentId);

        foreach (var attachment in assignment.Attachments)
        {
            await fileStorage.DeleteAsync(BlobPath(assignment, attachment));
        }

        db.Assignments.Remove(assignment);
        await db.SaveChangesAsync();
    }

    public async Task<AssignmentDto> AddAttachmentsAsync(Guid assignmentId, List<IFormFile> files)
    {
        var assignment = await GetOwnedAssignmentAsync(assignmentId);

        foreach (var file in files)
        {
            assignment.Attachments.Add(await BuildAttachmentAsync(assignment.TrainingId, assignment.Id, file));
        }

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(assignment.Id);
    }

    public async Task DeleteAttachmentAsync(Guid assignmentId, Guid attachmentId)
    {
        var assignment = await GetOwnedAssignmentAsync(assignmentId);

        var attachment = assignment.Attachments.FirstOrDefault(a => a.Id == attachmentId)
            ?? throw new NotFoundException($"Attachment '{attachmentId}' was not found.");

        await fileStorage.DeleteAsync(BlobPath(assignment, attachment));

        db.AssignmentAttachments.Remove(attachment);
        await db.SaveChangesAsync();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadAttachmentAsync(Guid assignmentId, Guid attachmentId)
    {
        var assignment = await db.Assignments
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == assignmentId)
            ?? throw new NotFoundException($"Assignment '{assignmentId}' was not found.");

        await EnsureViewAccessAsync(assignment.TrainingId);

        var attachment = assignment.Attachments.FirstOrDefault(a => a.Id == attachmentId)
            ?? throw new NotFoundException($"Attachment '{attachmentId}' was not found.");

        Stream stream;
        try
        {
            stream = await fileStorage.DownloadAsync(BlobPath(assignment, attachment));
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException("The attachment file is missing from storage.");
        }

        return (stream, attachment.ContentType, attachment.FileName);
    }

    private async Task<AssignmentAttachment> BuildAttachmentAsync(Guid trainingId, Guid assignmentId, IFormFile file)
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
            await fileStorage.UploadAsync($"{trainingId}/assignments/{assignmentId}/{storedFileName}", stream, file.ContentType);
        }

        return new AssignmentAttachment
        {
            AssignmentId = assignmentId,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length
        };
    }

    private static string BlobPath(Assignment assignment, AssignmentAttachment attachment)
        => $"{assignment.TrainingId}/assignments/{assignment.Id}/{attachment.StoredFileName}";

    private AssignmentDto ToDto(Assignment assignment, DateTime now)
    {
        var dto = mapper.Map<AssignmentDto>(assignment);
        dto.Status = AssignmentStatusCalculator.Compute(assignment.OpensAt, assignment.ClosesAt, now);
        return dto;
    }

    private async Task<AssignmentDto> ProjectSingleAsync(Guid assignmentId)
    {
        var assignment = await db.Assignments
            .Include(a => a.CreatedByUser)
            .Include(a => a.Attachments)
            .FirstAsync(a => a.Id == assignmentId);

        return ToDto(assignment, DateTime.UtcNow);
    }

    private async Task<Assignment> GetOwnedAssignmentAsync(Guid assignmentId)
    {
        var assignment = await db.Assignments
            .Include(a => a.Training)
            .Include(a => a.Attachments)
            .FirstOrDefaultAsync(a => a.Id == assignmentId)
            ?? throw new NotFoundException($"Assignment '{assignmentId}' was not found.");

        EnsureManagedDepartment(assignment.Training!.DepartmentId);

        return assignment;
    }

    private async Task EnsureViewAccessAsync(Guid trainingId)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin)
        {
            var managedDepartmentId = GetManagedDepartmentId();
            var belongsToDepartment = await db.Trainings.AnyAsync(t =>
                t.Id == trainingId && t.DepartmentId == managedDepartmentId);

            if (!belongsToDepartment)
            {
                throw new ForbiddenException("Department admins can only view assignments from their own department.");
            }

            return;
        }

        if (ViewOnlyRoles.Contains(currentUser.Role))
        {
            var userId = GetCurrentUserId();
            var isEnrolled = await db.Enrollments.AnyAsync(e => e.TrainingId == trainingId && e.UserId == userId);

            if (!isEnrolled)
            {
                throw new ForbiddenException("You can only view assignments for trainings you are enrolled in.");
            }
        }
    }

    private void EnsureManagedDepartment(Guid departmentId)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin && currentUser.DepartmentId != departmentId)
        {
            throw new ForbiddenException("Department admins can only manage assignments in their own department.");
        }
    }

    private Guid GetManagedDepartmentId()
        => currentUser.DepartmentId
            ?? throw new ForbiddenException("The signed-in department admin is not assigned to a department.");

    private Guid GetCurrentUserId()
        => currentUser.UserId
            ?? throw new UnauthorizedAccessException("A signed-in user is required.");
}
