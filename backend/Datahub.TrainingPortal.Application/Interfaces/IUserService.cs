using Datahub.TrainingPortal.Application.DTOs.Users;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IUserService
{
    Task<IReadOnlyList<UserDto>> GetAllAsync(Guid? departmentId = null);
    Task<IReadOnlyList<UserDto>> GetAssignableAsync();
    Task<UserDto> GetByIdAsync(Guid id);
    Task<UserDto> CreateAsync(CreateUserRequest request);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request);
    Task DeleteAsync(Guid id);
}
