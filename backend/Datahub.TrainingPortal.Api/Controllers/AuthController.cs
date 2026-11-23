using Datahub.TrainingPortal.Application.DTOs.Auth;
using Datahub.TrainingPortal.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    public async Task<ActionResult<LoginResponse>> Login(LoginRequest request)
    {
        var result = await authService.LoginAsync(request);
        return Ok(result);
    }
}
