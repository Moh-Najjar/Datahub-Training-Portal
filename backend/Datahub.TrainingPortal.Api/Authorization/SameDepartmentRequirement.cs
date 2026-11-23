using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Datahub.TrainingPortal.Api.Authorization;

public class SameDepartmentRequirement : IAuthorizationRequirement;

public class SameDepartmentHandler(IHttpContextAccessor httpContextAccessor)
    : AuthorizationHandler<SameDepartmentRequirement>
{
    protected override Task HandleRequirementAsync(
        AuthorizationHandlerContext context, SameDepartmentRequirement requirement)
    {
        if (context.User.IsInRole(RoleNames.SuperAdmin))
        {
            context.Succeed(requirement);
            return Task.CompletedTask;
        }

        var routeDepartmentId = httpContextAccessor.HttpContext?.Request.RouteValues["departmentId"]?.ToString();
        var userDepartmentId = context.User.FindFirst("departmentId")?.Value;

        if (routeDepartmentId is null ||
            (userDepartmentId is not null &&
             string.Equals(routeDepartmentId, userDepartmentId, StringComparison.OrdinalIgnoreCase)))
        {
            context.Succeed(requirement);
        }

        return Task.CompletedTask;
    }
}
