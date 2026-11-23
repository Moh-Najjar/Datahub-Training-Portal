using Datahub.TrainingPortal.Application.DTOs.Departments;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IDepartmentService
{
    Task<IReadOnlyList<DepartmentDto>> GetAllAsync();
    Task<DepartmentDto> GetByIdAsync(Guid id);
    Task<DepartmentDto> CreateAsync(CreateDepartmentRequest request);
    Task<DepartmentDto> UpdateAsync(Guid id, UpdateDepartmentRequest request);
    Task DeleteAsync(Guid id);
}
