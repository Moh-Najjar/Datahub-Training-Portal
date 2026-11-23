using Datahub.TrainingPortal.Application.DTOs.Auth;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> LoginAsync(LoginRequest request);
}
