using AutoMapper;
using AutoMapper.QueryableExtensions;
using Datahub.TrainingPortal.Application.DTOs.Trainings;
using Datahub.TrainingPortal.Application.Exceptions;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Datahub.TrainingPortal.Domain.Enums;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class TrainingService(AppDbContext db, IMapper mapper, ICurrentUserService currentUser) : ITrainingService
{
    private static readonly string[] ViewOnlyRoles = [RoleNames.Intern, RoleNames.Trainee, RoleNames.Employee];

    public async Task<IReadOnlyList<TrainingDto>> GetAllAsync(Guid? departmentId = null)
    {
        var query = db.Trainings.AsQueryable();

        if (currentUser.Role == RoleNames.DepartmentAdmin)
        {
            var managedDepartmentId = GetManagedDepartmentId();

            if (departmentId.HasValue && departmentId.Value != managedDepartmentId)
            {
                throw new ForbiddenException("Department admins can only view trainings from their own department.");
            }

            query = query.Where(t => t.DepartmentId == managedDepartmentId);
        }
        else if (ViewOnlyRoles.Contains(currentUser.Role))
        {
            var userId = GetCurrentUserId();
            query = query.Where(t => t.Status == TrainingStatus.Published && t.Enrollments.Any(e => e.UserId == userId));
        }

        if (departmentId.HasValue)
        {
            query = query.Where(t => t.DepartmentId == departmentId);
        }

        return await query
            .OrderBy(t => t.SortOrder)
            .ThenByDescending(t => t.CreatedAt)
            .ProjectTo<TrainingDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<TrainingDto> GetByIdAsync(Guid id)
    {
        await EnsureViewAccessAsync(id);

        return await db.Trainings
            .Where(t => t.Id == id)
            .ProjectTo<TrainingDto>(mapper.ConfigurationProvider)
            .FirstOrDefaultAsync()
            ?? throw new NotFoundException($"Training '{id}' was not found.");
    }

    public async Task<TrainingDto> CreateAsync(CreateTrainingRequest request)
    {
        EnsureManagedDepartment(request.DepartmentId);

        if (!await db.Departments.AnyAsync(d => d.Id == request.DepartmentId))
        {
            throw new NotFoundException($"Department '{request.DepartmentId}' was not found.");
        }

        var trainerId = currentUser.UserId
            ?? throw new UnauthorizedAccessException("A signed-in user is required to create a training.");

        var maxSortOrder = await db.Trainings
            .Where(t => t.DepartmentId == request.DepartmentId)
            .Select(t => (int?)t.SortOrder)
            .MaxAsync();
        var nextSortOrder = maxSortOrder.HasValue ? maxSortOrder.Value + 1 : 0;

        var training = new Training
        {
            Title = request.Title,
            Description = request.Description,
            DepartmentId = request.DepartmentId,
            TrainerId = trainerId,
            SortOrder = nextSortOrder
        };

        db.Trainings.Add(training);
        await db.SaveChangesAsync();

        return await GetByIdAsync(training.Id);
    }

    public async Task<TrainingDto> UpdateAsync(Guid id, UpdateTrainingRequest request)
    {
        var training = await FindAsync(id);
        EnsureManagedDepartment(training.DepartmentId);

        training.Title = request.Title;
        training.Description = request.Description;
        training.Status = request.Status;
        training.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var training = await FindAsync(id);
        EnsureManagedDepartment(training.DepartmentId);
        training.IsDeleted = true;
        training.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(ReorderTrainingsRequest request)
    {
        var trainings = await db.Trainings
            .Where(t => request.OrderedIds.Contains(t.Id))
            .ToListAsync();

        var trainingsById = trainings.ToDictionary(t => t.Id);

        for (var index = 0; index < request.OrderedIds.Count; index++)
        {
            if (!trainingsById.TryGetValue(request.OrderedIds[index], out var training))
            {
                throw new NotFoundException($"Training '{request.OrderedIds[index]}' was not found.");
            }

            EnsureManagedDepartment(training.DepartmentId);
            training.SortOrder = index;
            training.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
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
                throw new ForbiddenException("Department admins can only view trainings from their own department.");
            }

            return;
        }

        if (ViewOnlyRoles.Contains(currentUser.Role))
        {
            var userId = GetCurrentUserId();
            var isEnrolled = await db.Enrollments.AnyAsync(e => e.TrainingId == trainingId && e.UserId == userId);

            if (!isEnrolled)
            {
                throw new ForbiddenException("You can only view trainings you are enrolled in.");
            }

            var isPublished = await db.Trainings.AnyAsync(t => t.Id == trainingId && t.Status == TrainingStatus.Published);

            if (!isPublished)
            {
                throw new ForbiddenException("This training is not yet published.");
            }
        }
    }

    private void EnsureManagedDepartment(Guid departmentId)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin && currentUser.DepartmentId != departmentId)
        {
            throw new ForbiddenException("Department admins can only manage trainings in their own department.");
        }
    }

    private Guid GetManagedDepartmentId()
        => currentUser.DepartmentId
            ?? throw new ForbiddenException("The signed-in department admin is not assigned to a department.");

    private Guid GetCurrentUserId()
        => currentUser.UserId
            ?? throw new UnauthorizedAccessException("A signed-in user is required.");

    private async Task<Training> FindAsync(Guid id)
    {
        return await db.Trainings.FirstOrDefaultAsync(t => t.Id == id)
            ?? throw new NotFoundException($"Training '{id}' was not found.");
    }
}
