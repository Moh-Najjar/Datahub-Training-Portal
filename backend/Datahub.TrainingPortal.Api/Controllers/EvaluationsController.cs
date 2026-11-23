using Datahub.TrainingPortal.Application.DTOs.Evaluations;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/users/{userId:guid}/evaluations")]
[Authorize(Policy = PolicyNames.DepartmentManagement)]
public class EvaluationsController(IEvaluationService evaluationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<EvaluationDto>>> GetForUser(Guid userId)
        => Ok(await evaluationService.GetForUserAsync(userId));

    [HttpPost]
    public async Task<ActionResult<EvaluationDto>> Create(Guid userId, CreateEvaluationRequest request)
    {
        var result = await evaluationService.CreateAsync(userId, request);
        return Ok(result);
    }

    [HttpPut("{evaluationId:guid}")]
    public async Task<ActionResult<EvaluationDto>> Update(Guid userId, Guid evaluationId, UpdateEvaluationRequest request)
    {
        var result = await evaluationService.UpdateAsync(evaluationId, request);
        return Ok(result);
    }

    [HttpDelete("{evaluationId:guid}")]
    public async Task<IActionResult> Delete(Guid userId, Guid evaluationId)
    {
        await evaluationService.DeleteAsync(evaluationId);
        return NoContent();
    }
}
