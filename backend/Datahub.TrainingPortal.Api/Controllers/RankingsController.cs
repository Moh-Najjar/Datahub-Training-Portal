using Datahub.TrainingPortal.Application.DTOs.Evaluations;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/rankings")]
[Authorize(Policy = PolicyNames.DepartmentManagement)]
public class RankingsController(IEvaluationService evaluationService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<RankingEntryDto>>> GetAll()
        => Ok(await evaluationService.GetRankingsAsync());
}
