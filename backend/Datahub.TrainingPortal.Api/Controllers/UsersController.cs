using Datahub.TrainingPortal.Application.DTOs.Users;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController(IUserService userService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAll([FromQuery] Guid? departmentId)
        => Ok(await userService.GetAllAsync(departmentId));

    [HttpGet("assignable")]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<IReadOnlyList<UserDto>>> GetAssignable()
        => Ok(await userService.GetAssignableAsync());

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<UserDto>> GetById(Guid id)
        => Ok(await userService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<UserDto>> Create(CreateUserRequest request)
    {
        var result = await userService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<UserDto>> Update(Guid id, UpdateUserRequest request)
        => Ok(await userService.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await userService.DeleteAsync(id);
        return NoContent();
    }
}
