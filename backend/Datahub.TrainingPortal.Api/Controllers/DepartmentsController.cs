using Datahub.TrainingPortal.Application.DTOs.Departments;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DepartmentsController(IDepartmentService departmentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<IReadOnlyList<DepartmentDto>>> GetAll()
        => Ok(await departmentService.GetAllAsync());

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<DepartmentDto>> GetById(Guid id)
        => Ok(await departmentService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Policy = PolicyNames.SuperAdminOnly)]
    public async Task<ActionResult<DepartmentDto>> Create(CreateDepartmentRequest request)
    {
        var result = await departmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.SuperAdminOnly)]
    public async Task<ActionResult<DepartmentDto>> Update(Guid id, UpdateDepartmentRequest request)
        => Ok(await departmentService.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PolicyNames.SuperAdminOnly)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await departmentService.DeleteAsync(id);
        return NoContent();
    }
}
