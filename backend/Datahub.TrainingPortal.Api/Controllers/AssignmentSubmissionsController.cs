using Datahub.TrainingPortal.Application.DTOs.Assignments;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/trainings/{trainingId:guid}/assignments/{assignmentId:guid}")]
[Authorize]
public class AssignmentSubmissionsController(IAssignmentSubmissionService submissionService) : ControllerBase
{
    [HttpGet("submission")]
    public async Task<ActionResult<AssignmentSubmissionDto?>> GetMine(Guid trainingId, Guid assignmentId)
        => Ok(await submissionService.GetMineAsync(trainingId, assignmentId));

    [HttpGet("submissions")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<IReadOnlyList<AssignmentSubmissionOverviewDto>>> GetAll(Guid trainingId, Guid assignmentId)
        => Ok(await submissionService.GetAllForAssignmentAsync(trainingId, assignmentId));

    [HttpPut("submission")]
    [RequestSizeLimit(AppConstants.MaxFileSizeBytes)]
    public async Task<ActionResult<AssignmentSubmissionDto>> UpsertMine(
        Guid trainingId, Guid assignmentId, [FromForm] UpsertAssignmentSubmissionRequest request)
    {
        var result = await submissionService.UpsertMineAsync(trainingId, assignmentId, request);
        return Ok(result);
    }

    [HttpDelete("submission/attachments/{attachmentId:guid}")]
    public async Task<IActionResult> DeleteAttachment(Guid trainingId, Guid assignmentId, Guid attachmentId)
    {
        await submissionService.DeleteAttachmentAsync(trainingId, assignmentId, attachmentId);
        return NoContent();
    }

    [HttpGet("submission/attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> DownloadAttachment(Guid trainingId, Guid assignmentId, Guid attachmentId)
    {
        var (stream, contentType, fileName) = await submissionService.DownloadAttachmentAsync(trainingId, assignmentId, attachmentId);
        return File(stream, contentType, fileName);
    }
}
