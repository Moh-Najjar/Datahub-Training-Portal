using AutoMapper;
using Datahub.TrainingPortal.Application.DTOs.Evaluations;
using Datahub.TrainingPortal.Application.Exceptions;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class EvaluationService(
    AppDbContext db,
    IMapper mapper,
    ICurrentUserService currentUser) : IEvaluationService
{
    private static readonly string[] EvaluableRoles = [RoleNames.Trainee, RoleNames.Employee, RoleNames.Intern];

    public async Task<IReadOnlyList<EvaluationDto>> GetForUserAsync(Guid userId)
    {
        var user = await GetEvaluableUserAsync(userId);
        await EnsureCanViewUserEvaluationsAsync(user);

        var evaluations = await db.Evaluations
            .Where(e => e.UserId == userId)
            .Include(e => e.User).ThenInclude(u => u!.Department)
            .Include(e => e.EvaluatedByUser)
            .Include(e => e.Categories)
            .Include(e => e.Enrollment!).ThenInclude(en => en.Training!).ThenInclude(t => t.Department)
            .OrderByDescending(e => e.EvaluatedAt)
            .ToListAsync();

        return evaluations.Select(e => mapper.Map<EvaluationDto>(e)).ToList();
    }

    public async Task<EvaluationDto> CreateAsync(Guid userId, CreateEvaluationRequest request)
    {
        var user = await GetEvaluableUserAsync(userId);

        Enrollment? enrollment = null;
        if (request.EnrollmentId.HasValue)
        {
            enrollment = await db.Enrollments
                .Include(e => e.Training)
                .FirstOrDefaultAsync(e => e.Id == request.EnrollmentId.Value)
                ?? throw new NotFoundException($"Enrollment '{request.EnrollmentId}' was not found.");

            if (enrollment.UserId != userId)
            {
                throw new ArgumentException("The enrollment does not belong to this user.");
            }
        }

        EnsureCanEvaluate(user, enrollment);

        var evaluation = new Evaluation
        {
            UserId = userId,
            EnrollmentId = enrollment?.Id,
            EvaluatedByUserId = GetCurrentUserId(),
            EvaluatedAt = request.EvaluatedAt,
            Categories = request.Categories.Select((c, index) => new EvaluationCategoryScore
            {
                CategoryName = c.CategoryName,
                Criteria = c.Criteria,
                Score = c.Score,
                SortOrder = index
            }).ToList()
        };

        db.Evaluations.Add(evaluation);
        await db.SaveChangesAsync();

        return await ProjectSingleAsync(evaluation.Id);
    }

    public async Task<EvaluationDto> UpdateAsync(Guid evaluationId, UpdateEvaluationRequest request)
    {
        var evaluation = await GetOwnedEvaluationAsync(evaluationId);

        evaluation.EvaluatedAt = request.EvaluatedAt;
        evaluation.UpdatedAt = DateTime.UtcNow;

        db.EvaluationCategoryScores.RemoveRange(evaluation.Categories);
        db.EvaluationCategoryScores.AddRange(request.Categories.Select((c, index) => new EvaluationCategoryScore
        {
            EvaluationId = evaluation.Id,
            CategoryName = c.CategoryName,
            Criteria = c.Criteria,
            Score = c.Score,
            SortOrder = index
        }));

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(evaluation.Id);
    }

    public async Task DeleteAsync(Guid evaluationId)
    {
        var evaluation = await GetOwnedEvaluationAsync(evaluationId);

        db.Evaluations.Remove(evaluation);
        await db.SaveChangesAsync();
    }

    public async Task<IReadOnlyList<RankingEntryDto>> GetRankingsAsync()
    {
        var usersQuery = db.Users.Where(u =>
            (u.Role == RoleNames.Trainee || u.Role == RoleNames.Employee || u.Role == RoleNames.Intern)
            && u.IsActive);

        Guid? managedDepartmentId = null;

        if (currentUser.Role == RoleNames.DepartmentAdmin)
        {
            managedDepartmentId = GetManagedDepartmentId();
            var deptId = managedDepartmentId.Value;
            usersQuery = usersQuery.Where(u =>
                u.DepartmentId == deptId || u.Enrollments.Any(e => e.Training.DepartmentId == deptId));
        }

        var users = await usersQuery.Include(u => u.Department).ToListAsync();
        var userIds = users.Select(u => u.Id).ToList();
        var homeDepartmentByUser = users.ToDictionary(u => u.Id, u => u.DepartmentId);

        var evaluations = await db.Evaluations
            .Where(e => userIds.Contains(e.UserId))
            .Include(e => e.Categories)
            .Include(e => e.Enrollment!).ThenInclude(en => en.Training!)
            .ToListAsync();

        if (managedDepartmentId.HasValue)
        {
            var deptId = managedDepartmentId.Value;
            evaluations = evaluations
                .Where(e =>
                {
                    var departmentContext = e.Enrollment != null
                        ? e.Enrollment.Training!.DepartmentId
                        : homeDepartmentByUser.GetValueOrDefault(e.UserId);
                    return departmentContext == deptId;
                })
                .ToList();
        }

        var evaluationsByUser = evaluations.GroupBy(e => e.UserId).ToDictionary(g => g.Key, g => g.ToList());

        var entries = users.Select(u =>
        {
            evaluationsByUser.TryGetValue(u.Id, out var userEvaluations);
            var latest = userEvaluations?.OrderByDescending(e => e.EvaluatedAt).FirstOrDefault();

            return new RankingEntryDto
            {
                UserId = u.Id,
                UserName = u.FullName,
                Role = u.Role,
                DepartmentName = u.Department?.Name,
                LatestTotal = latest?.Categories.Sum(c => c.Score),
                LatestMaxTotal = latest?.Categories.Count * 10,
                LatestEvaluatedAt = latest?.EvaluatedAt,
                EvaluationCount = userEvaluations?.Count ?? 0
            };
        });

        return entries
            .OrderByDescending(e => e.LatestTotal.HasValue)
            .ThenByDescending(e => e.LatestTotal)
            .ThenBy(e => e.UserName)
            .ToList();
    }

    private async Task<User> GetEvaluableUserAsync(Guid userId)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId)
            ?? throw new NotFoundException($"User '{userId}' was not found.");

        if (!EvaluableRoles.Contains(user.Role))
        {
            throw new ArgumentException("Only trainees, employees, and interns can be evaluated.");
        }

        return user;
    }

    private async Task<Evaluation> GetOwnedEvaluationAsync(Guid evaluationId)
    {
        var evaluation = await db.Evaluations
            .Include(e => e.User)
            .Include(e => e.Categories)
            .Include(e => e.Enrollment!).ThenInclude(en => en.Training!)
            .FirstOrDefaultAsync(e => e.Id == evaluationId)
            ?? throw new NotFoundException($"Evaluation '{evaluationId}' was not found.");

        var departmentContext = evaluation.Enrollment != null
            ? evaluation.Enrollment.Training!.DepartmentId
            : evaluation.User!.DepartmentId;

        EnsureManagedDepartment(departmentContext);

        return evaluation;
    }

    private async Task<EvaluationDto> ProjectSingleAsync(Guid evaluationId)
    {
        var evaluation = await db.Evaluations
            .Include(e => e.User).ThenInclude(u => u!.Department)
            .Include(e => e.EvaluatedByUser)
            .Include(e => e.Categories)
            .Include(e => e.Enrollment!).ThenInclude(en => en.Training!).ThenInclude(t => t.Department)
            .FirstAsync(e => e.Id == evaluationId);

        return mapper.Map<EvaluationDto>(evaluation);
    }

    private void EnsureCanEvaluate(User user, Enrollment? enrollment)
    {
        if (currentUser.Role != RoleNames.DepartmentAdmin)
        {
            return;
        }

        var managedDepartmentId = GetManagedDepartmentId();

        if (enrollment != null)
        {
            if (enrollment.Training!.DepartmentId != managedDepartmentId)
            {
                throw new ForbiddenException(
                    "Department admins can only record evaluations for enrollments in their own department's trainings.");
            }

            return;
        }

        EnsureManagedDepartment(user.DepartmentId);
    }

    private async Task EnsureCanViewUserEvaluationsAsync(User user)
    {
        if (currentUser.Role != RoleNames.DepartmentAdmin)
        {
            return;
        }

        var managedDepartmentId = GetManagedDepartmentId();

        if (user.DepartmentId == managedDepartmentId)
        {
            return;
        }

        var hasAccess = await db.Enrollments.AnyAsync(e =>
            e.UserId == user.Id && e.Training.DepartmentId == managedDepartmentId);

        if (!hasAccess)
        {
            throw new ForbiddenException(
                "Department admins can only view evaluations for users in their own department or enrolled in their department's trainings.");
        }
    }

    private void EnsureManagedDepartment(Guid? departmentId)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin && currentUser.DepartmentId != departmentId)
        {
            throw new ForbiddenException("Department admins can only manage evaluations for users in their own department.");
        }
    }

    private Guid GetManagedDepartmentId()
        => currentUser.DepartmentId
            ?? throw new ForbiddenException("The signed-in department admin is not assigned to a department.");

    private Guid GetCurrentUserId()
        => currentUser.UserId
            ?? throw new UnauthorizedAccessException("A signed-in user is required.");
}
