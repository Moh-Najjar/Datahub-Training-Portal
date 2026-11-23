using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Application.DTOs.Materials;

public class TrainingMaterialDto
{
    public Guid Id { get; set; }
    public Guid TrainingId { get; set; }
    public MaterialType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public string? TextContent { get; set; }
    public List<MaterialFileItemDto> FileItems { get; set; } = [];
    public List<MaterialLinkItemDto> LinkItems { get; set; } = [];
    public Guid UploadedByUserId { get; set; }
    public string? UploadedByUserName { get; set; }
    public DateTime CreatedAt { get; set; }
}
