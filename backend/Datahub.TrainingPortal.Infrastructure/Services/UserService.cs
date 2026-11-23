using AutoMapper;
using AutoMapper.QueryableExtensions;
using Datahub.TrainingPortal.Application.DTOs.Users;
using Datahub.TrainingPortal.Application.Exceptions;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class UserService(
    AppDbContext db,
    IMapper mapper,
    IPasswordHasher passwordHasher,
    ICurrentUserService currentUser) : IUserService
{
    private static readonly string[] DepartmentAdminAssignableRoles =
        [RoleNames.Intern, RoleNames.Trainee, RoleNames.Employee];

    public async Task<IReadOnlyList<UserDto>> GetAllAsync(Guid? departmentId = null)
    {
        var query = db.Users.Include(u => u.Department).AsQueryable();

        if (currentUser.Role == RoleNames.DepartmentAdmin)
        {
            var currentDepartmentId = GetManagedDepartmentId();

            if (departmentId.HasValue && departmentId.Value != currentDepartmentId)
            {
                throw new ForbiddenException("Department admins can only view users from their own department.");
            }

            query = query.Where(u => u.DepartmentId == currentDepartmentId);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(u => u.DepartmentId == departmentId);
        }

        return await query
            .OrderBy(u => u.FullName)
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<IReadOnlyList<UserDto>> GetAssignableAsync()
    {
        return await db.Users
            .Include(u => u.Department)
            .Where(u => u.IsActive &&
                (u.Role == RoleNames.Intern || u.Role == RoleNames.Trainee || u.Role == RoleNames.Employee))
            .OrderBy(u => u.FullName)
            .ProjectTo<UserDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<UserDto> GetByIdAsync(Guid id)
    {
        var user = await db.Users.Include(u => u.Department).FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException($"User '{id}' was not found.");

        await EnsureCanViewAsync(user);

        return mapper.Map<UserDto>(user);
    }

    public async Task<UserDto> CreateAsync(CreateUserRequest request)
    {
        var email = NormalizeEmail(request.Email);
        var departmentId = ResolveDepartmentId(request.DepartmentId);

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            throw new ConflictException($"A user with email '{email}' already exists.");
        }

        EnsureRoleCanBeManaged(request.Role);

        var user = new User
        {
            FullName = request.FullName,
            Email = email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Role = request.Role,
            DepartmentId = departmentId
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return await GetByIdAsync(user.Id);
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException($"User '{id}' was not found.");

        EnsureUserAccess(user);
        EnsureRoleCanBeManaged(request.Role);

        var email = NormalizeEmail(request.Email);

        if (await db.Users.AnyAsync(u => u.Email == email && u.Id != id))
        {
            throw new ConflictException($"A user with email '{email}' already exists.");
        }

        user.FullName = request.FullName;
        user.Email = email;
        user.Role = request.Role;
        user.IsActive = request.IsActive;
        user.DepartmentId = ResolveDepartmentId(request.DepartmentId);
        user.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await GetByIdAsync(user.Id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == id)
            ?? throw new NotFoundException($"User '{id}' was not found.");

        EnsureUserAccess(user);

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private void EnsureUserAccess(User user)
    {
        if (currentUser.Role != RoleNames.DepartmentAdmin)
        {
            return;
        }

        var managedDepartmentId = GetManagedDepartmentId();

        if (user.DepartmentId != managedDepartmentId)
        {
            throw new ForbiddenException("Department admins can only access users from their own department.");
        }
    }

    private async Task EnsureCanViewAsync(User user)
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
                "Department admins can only view users from their own department or enrolled in their department's trainings.");
        }
    }

    private void EnsureRoleCanBeManaged(string role)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin && !DepartmentAdminAssignableRoles.Contains(role))
        {
            throw new ForbiddenException(
                "Department admins can only assign the Intern, Trainee, or Employee roles.");
        }
    }

    private Guid? ResolveDepartmentId(Guid? requestedDepartmentId)
    {
        if (currentUser.Role != RoleNames.DepartmentAdmin)
        {
            return requestedDepartmentId;
        }

        var managedDepartmentId = GetManagedDepartmentId();

        if (requestedDepartmentId.HasValue && requestedDepartmentId.Value != managedDepartmentId)
        {
            throw new ForbiddenException("Department admins can only manage users in their own department.");
        }

        return managedDepartmentId;
    }

    private Guid GetManagedDepartmentId()
        => currentUser.DepartmentId
            ?? throw new ForbiddenException("The signed-in department admin is not assigned to a department.");

    private static string NormalizeEmail(string email)
        => email.Trim().Replace(" ", "");
}
