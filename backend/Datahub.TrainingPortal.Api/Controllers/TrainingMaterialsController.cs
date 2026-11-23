using Datahub.TrainingPortal.Application.DTOs.Materials;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Datahub.TrainingPortal.Api.Controllers;

[ApiController]
[Route("api/trainings/{trainingId:guid}/materials")]
[Authorize]
public class TrainingMaterialsController(ITrainingMaterialService materialService) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<TrainingMaterialDto>>> GetAll(Guid trainingId)
        => Ok(await materialService.GetForTrainingAsync(trainingId));

    [HttpPost("file")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    [RequestSizeLimit(AppConstants.MaxFileSizeBytes)]
    public async Task<ActionResult<TrainingMaterialDto>> AddFileGroup(Guid trainingId, [FromForm] CreateFileMaterialRequest request)
    {
        var result = await materialService.AddFileGroupAsync(trainingId, request);
        return Ok(result);
    }

    [HttpPost("text")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<TrainingMaterialDto>> AddText(Guid trainingId, CreateTextMaterialRequest request)
    {
        var result = await materialService.AddTextAsync(trainingId, request);
        return Ok(result);
    }

    [HttpPost("link")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<TrainingMaterialDto>> AddLink(Guid trainingId, CreateLinkMaterialRequest request)
    {
        var result = await materialService.AddLinkAsync(trainingId, request);
        return Ok(result);
    }

    [HttpPost("{materialId:guid}/files")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    [RequestSizeLimit(AppConstants.MaxFileSizeBytes)]
    public async Task<ActionResult<TrainingMaterialDto>> AddFileItems(Guid trainingId, Guid materialId, [FromForm] AddMaterialFileItemsRequest request)
    {
        var result = await materialService.AddFileItemsAsync(materialId, request);
        return Ok(result);
    }

    [HttpPut("{materialId:guid}/files/{fileItemId:guid}")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<TrainingMaterialDto>> UpdateFileItem(Guid trainingId, Guid materialId, Guid fileItemId, UpdateMaterialFileItemRequest request)
    {
        var result = await materialService.UpdateFileItemAsync(materialId, fileItemId, request);
        return Ok(result);
    }

    [HttpDelete("{materialId:guid}/files/{fileItemId:guid}")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<IActionResult> DeleteFileItem(Guid trainingId, Guid materialId, Guid fileItemId)
    {
        await materialService.DeleteFileItemAsync(materialId, fileItemId);
        return NoContent();
    }

    [HttpGet("{materialId:guid}/files/{fileItemId:guid}/download")]
    public async Task<IActionResult> DownloadFileItem(Guid trainingId, Guid materialId, Guid fileItemId)
    {
        var (stream, contentType, fileName) = await materialService.DownloadFileItemAsync(materialId, fileItemId);
        return File(stream, contentType, fileName);
    }

    [HttpPut("{materialId:guid}/text")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<TrainingMaterialDto>> UpdateText(Guid trainingId, Guid materialId, CreateTextMaterialRequest request)
    {
        var result = await materialService.UpdateTextAsync(materialId, request);
        return Ok(result);
    }

    [HttpPut("{materialId:guid}/link")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<TrainingMaterialDto>> UpdateLink(Guid trainingId, Guid materialId, CreateLinkMaterialRequest request)
    {
        var result = await materialService.UpdateLinkAsync(materialId, request);
        return Ok(result);
    }

    [HttpPut("{materialId:guid}/file")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<ActionResult<TrainingMaterialDto>> UpdateFile(Guid trainingId, Guid materialId, UpdateFileMaterialRequest request)
    {
        var result = await materialService.UpdateFileTitleAsync(materialId, request);
        return Ok(result);
    }

    [HttpDelete("{materialId:guid}")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<IActionResult> Delete(Guid trainingId, Guid materialId)
    {
        await materialService.DeleteAsync(materialId);
        return NoContent();
    }

    [HttpPut("reorder")]
    [Authorize(Policy = PolicyNames.TrainingAuthoring)]
    public async Task<IActionResult> Reorder(Guid trainingId, ReorderMaterialsRequest request)
    {
        await materialService.ReorderAsync(trainingId, request);
        return NoContent();
    }
}
