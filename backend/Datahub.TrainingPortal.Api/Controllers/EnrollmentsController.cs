using Datahub.TrainingPortal.Application.DTOs.Enrollments;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class EnrollmentsController(IEnrollmentService enrollmentService) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<IReadOnlyList<EnrollmentDto>>> GetAll(
        [FromQuery] Guid? trainingId, [FromQuery] Guid? userId)
        => Ok(await enrollmentService.GetAllAsync(trainingId, userId));

    [HttpGet("{id:guid}")]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<EnrollmentDto>> GetById(Guid id)
        => Ok(await enrollmentService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<EnrollmentDto>> Create(CreateEnrollmentRequest request)
    {
        var result = await enrollmentService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}/status")]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<ActionResult<EnrollmentDto>> UpdateStatus(Guid id, UpdateEnrollmentStatusRequest request)
        => Ok(await enrollmentService.UpdateStatusAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PolicyNames.DepartmentManagement)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await enrollmentService.DeleteAsync(id);
        return NoContent();
    }
}
