using Datahub.TrainingPortal.Domain.Entities;

namespace Datahub.TrainingPortal.Application.Interfaces;

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAt) GenerateToken(User user);
}
