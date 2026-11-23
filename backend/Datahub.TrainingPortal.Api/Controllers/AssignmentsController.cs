using Datahub.TrainingPortal.Application.DTOs.Assignments;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/trainings/{trainingId:guid}/assignments")]
[Authorize]
public class AssignmentsController(IAssignmentService assignmentService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<AssignmentDto>>> GetAll(Guid trainingId)
        => Ok(await assignmentService.GetForTrainingAsync(trainingId));

    [HttpPost]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    [RequestSizeLimit(AppConstants.MaxFileSizeBytes)]
    public async Task<ActionResult<AssignmentDto>> Create(Guid trainingId, [FromForm] CreateAssignmentRequest request)
    {
        var result = await assignmentService.CreateAsync(trainingId, request);
        return Ok(result);
    }

    [HttpPut("{assignmentId:guid}")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<AssignmentDto>> Update(Guid trainingId, Guid assignmentId, UpdateAssignmentRequest request)
    {
        var result = await assignmentService.UpdateAsync(assignmentId, request);
        return Ok(result);
    }

    [HttpDelete("{assignmentId:guid}")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<IActionResult> Delete(Guid trainingId, Guid assignmentId)
    {
        await assignmentService.DeleteAsync(assignmentId);
        return NoContent();
    }

    [HttpPost("{assignmentId:guid}/attachments")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    [RequestSizeLimit(AppConstants.MaxFileSizeBytes)]
    public async Task<ActionResult<AssignmentDto>> AddAttachments(Guid trainingId, Guid assignmentId, List<IFormFile> files)
    {
        var result = await assignmentService.AddAttachmentsAsync(assignmentId, files);
        return Ok(result);
    }

    [HttpDelete("{assignmentId:guid}/attachments/{attachmentId:guid}")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<IActionResult> DeleteAttachment(Guid trainingId, Guid assignmentId, Guid attachmentId)
    {
        await assignmentService.DeleteAttachmentAsync(assignmentId, attachmentId);
        return NoContent();
    }

    [HttpGet("{assignmentId:guid}/attachments/{attachmentId:guid}/download")]
    public async Task<IActionResult> DownloadAttachment(Guid trainingId, Guid assignmentId, Guid attachmentId)
    {
        var (stream, contentType, fileName) = await assignmentService.DownloadAttachmentAsync(assignmentId, attachmentId);
        return File(stream, contentType, fileName);
    }
}
