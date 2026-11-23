using Datahub.TrainingPortal.Application.DTOs.Trainings;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TrainingsController(ITrainingService trainingService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrainingDto>>> GetAll([FromQuery] Guid? departmentId)
        => Ok(await trainingService.GetAllAsync(departmentId));

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TrainingDto>> GetById(Guid id)
        => Ok(await trainingService.GetByIdAsync(id));

    [HttpPost]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<TrainingDto>> Create(CreateTrainingRequest request)
    {
        var result = await trainingService.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<TrainingDto>> Update(Guid id, UpdateTrainingRequest request)
        => Ok(await trainingService.UpdateAsync(id, request));

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<IActionResult> Delete(Guid id)
    {
        await trainingService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPut("reorder")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<IActionResult> Reorder(ReorderTrainingsRequest request)
    {
        await trainingService.ReorderAsync(request);
        return NoContent();
    }
}
