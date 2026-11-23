using AutoMapper;
using AutoMapper.QueryableExtensions;
using Datahub.TrainingPortal.Application.DTOs.Enrollments;
using Datahub.TrainingPortal.Application.Exceptions;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Datahub.TrainingPortal.Domain.Enums;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class EnrollmentService(AppDbContext db, IMapper mapper, ICurrentUserService currentUser) : IEnrollmentService
{
    private static readonly string[] DepartmentAdminAssignableRoles =
        [RoleNames.Intern, RoleNames.Trainee, RoleNames.Employee];

    public async Task<IReadOnlyList<EnrollmentDto>> GetAllAsync(Guid? trainingId = null, Guid? userId = null)
    {
        var query = db.Enrollments.AsQueryable();

        if (currentUser.Role == RoleNames.DepartmentAdmin)
        {
            var departmentId = GetCurrentDepartmentId();
            query = query.Where(e => e.Training.DepartmentId == departmentId);
        }

        if (trainingId.HasValue)
        {
            query = query.Where(e => e.TrainingId == trainingId);
        }

        if (userId.HasValue)
        {
            query = query.Where(e => e.UserId == userId);
        }

        return await query
            .OrderByDescending(e => e.AssignedAt)
            .ProjectTo<EnrollmentDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<EnrollmentDto> GetByIdAsync(Guid id)
    {
        var enrollment = await db.Enrollments
            .Where(e => e.Id == id)
            .ProjectTo<EnrollmentDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Enrollment '{id}' was not found.");

        await EnsureDepartmentAdminCanAccessAsync(enrollment.TrainingId, enrollment.UserId);

        return enrollment;
    }

    public async Task<EnrollmentDto> CreateAsync(CreateEnrollmentRequest request)
    {
        var training = await db.Trainings.FirstOrDefaultAsync(t => t.Id == request.TrainingId)
            ?? throw new NotFoundException($"Training '{request.TrainingId}' was not found.");

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == request.UserId)
            ?? throw new NotFoundException($"User '{request.UserId}' was not found.");

        EnsureDepartmentAdminCanAssign(training, user);

        if (await db.Enrollments.AnyAsync(e => e.TrainingId == request.TrainingId && e.UserId == request.UserId))
        {
            throw new ConflictException("This user is already enrolled in this training.");
        }

        var assignedByUserId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("A signed-in user is required to assign a training.");

        var enrollment = new Enrollment
        {
            TrainingId = request.TrainingId,
            UserId = request.UserId,
            AssignedByUserId = assignedByUserId
        };

        db.Enrollments.Add(enrollment);
        await db.SaveChangesAsync();

        return await GetByIdAsync(enrollment.Id);
    }

    public async Task<EnrollmentDto> UpdateStatusAsync(Guid id, UpdateEnrollmentStatusRequest request)
    {
        var enrollment = await FindAsync(id);
        await EnsureDepartmentAdminCanAccessAsync(enrollment.TrainingId, enrollment.UserId);

        enrollment.Status = request.Status;
        enrollment.CompletedAt = request.Status == EnrollmentStatus.Completed ? DateTime.UtcNow : null;
        enrollment.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var enrollment = await FindAsync(id);
        await EnsureDepartmentAdminCanAccessAsync(enrollment.TrainingId, enrollment.UserId);
        enrollment.IsDeleted = true;
        enrollment.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private void EnsureDepartmentAdminCanAssign(Training training, User user)
    {
        if (currentUser.Role != RoleNames.DepartmentAdmin)
        {
            return;
        }

        var departmentId = GetCurrentDepartmentId();

        if (training.DepartmentId != departmentId)
        {
            throw new ForbiddenException("Department admins can only assign trainings owned by their own department.");
        }

        if (!DepartmentAdminAssignableRoles.Contains(user.Role))
        {
            throw new ForbiddenException("Department admins can only assign trainings to Intern, Trainee, or Employee users.");
        }
    }

    private async Task EnsureDepartmentAdminCanAccessAsync(Guid trainingId, Guid userId)
    {
        if (currentUser.Role != RoleNames.DepartmentAdmin)
        {
            return;
        }

        var departmentId = GetCurrentDepartmentId();
        var hasAccess = await db.Enrollments.AnyAsync(e =>
            e.TrainingId == trainingId &&
            e.UserId == userId &&
            e.Training.DepartmentId == departmentId);

        if (!hasAccess)
        {
            throw new ForbiddenException("Department admins can only access enrollments in trainings owned by their own department.");
        }
    }

    private Guid GetCurrentDepartmentId()
        => currentUser.DepartmentId
            ?? throw new ForbiddenException("The signed-in department admin is not assigned to a department.");

    private async Task<Enrollment> FindAsync(Guid id)
    {
        return await db.Enrollments.FirstOrDefaultAsync(e => e.Id == id)
            ?? throw new NotFoundException($"Enrollment '{id}' was not found.");
    }
}
