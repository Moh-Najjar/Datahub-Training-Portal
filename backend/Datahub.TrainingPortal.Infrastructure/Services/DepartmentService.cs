using AutoMapper;
using AutoMapper.QueryableExtensions;
using Datahub.TrainingPortal.Application.DTOs.Departments;
using Datahub.TrainingPortal.Application.Exceptions;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class DepartmentService(AppDbContext db, IMapper mapper, ICurrentUserService currentUser) : IDepartmentService
{
    public async Task<IReadOnlyList<DepartmentDto>> GetAllAsync()
    {
        var query = db.Departments.AsQueryable();

        if (currentUser.Role == RoleNames.DepartmentAdmin)
        {
            var departmentId = GetCurrentDepartmentId();
            query = query.Where(d => d.Id == departmentId);
        }

        return await query
            .OrderBy(d => d.Name)
            .ProjectTo<DepartmentDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<DepartmentDto> GetByIdAsync(Guid id)
    {
        EnsureDepartmentAccess(id);

        var department = await db.Departments
            .Include(d => d.Users)
            .Include(d => d.Trainings)
            .FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException($"Department '{id}' was not found.");

        return mapper.Map<DepartmentDto>(department);
    }

    public async Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request)
    {
        if (await db.Departments.AnyAsync(d => d.Name == request.Name))
        {
            throw new ConflictException($"A department named '{request.Name}' already exists.");
        }

        var department = new Department { Name = request.Name };
        db.Departments.Add(department);
        await db.SaveChangesAsync();

        return await GetByIdAsync(department.Id);
    }

    public async Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentRequest request)
    {
        var department = await db.Departments.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException($"Department '{id}' was not found.");

        if (await db.Departments.AnyAsync(d => d.Name == request.Name && d.Id != id))
        {
            throw new ConflictException($"A department named '{request.Name}' already exists.");
        }

        department.Name = request.Name;
        department.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await GetByIdAsync(id);
    }

    public async Task DeleteAsync(Guid id)
    {
        var department = await db.Departments.FirstOrDefaultAsync(d => d.Id == id)
            ?? throw new NotFoundException($"Department '{id}' was not found.");

        department.IsDeleted = true;
        department.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();
    }

    private void EnsureDepartmentAccess(Guid departmentId)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin && currentUser.DepartmentId != departmentId)
        {
            throw new ForbiddenException("Department admins can only access their own department.");
        }
    }

    private Guid GetCurrentDepartmentId()
        => currentUser.DepartmentId
            ?? throw new ForbiddenException("The signed-in department admin is not assigned to a department.");
}
