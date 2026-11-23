using System.Security.Claims;
using Datahub.TrainingPortal.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace Datahub.TrainingPortal.Infrastructure.Auth;

public class CurrentUserService(IHttpContextAccessor httpContextAccessor) : ICurrentUserService
{
    private ClaimsPrincipal? User => httpContextAccessor.HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }

    public string? Role => User?.FindFirst(ClaimTypes.Role)?.Value;

    public Guid? DepartmentId
    {
        get
        {
            var value = User?.FindFirst("departmentId")?.Value;
            return Guid.TryParse(value, out var id) ? id : null;
        }
    }
}
