using Datahub.TrainingPortal.Application.DTOs.Auth;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class AuthService(AppDbContext db, IPasswordHasher passwordHasher, IJwtTokenService jwtTokenService)
    : IAuthService
{
    public async Task<LoginResponse> LoginAsync(LoginRequest request)
    {
        var email = request.Email.Trim().Replace(" ", "");
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user is null || !user.IsActive || !passwordHasher.Verify(request.Password, user.PasswordHash))
        {
            throw new UnauthorizedAccessException("Invalid email or password.");
        }

        var (token, expiresAt) = jwtTokenService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            ExpiresAt = expiresAt,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            DepartmentId = user.DepartmentId
        };
    }
}
