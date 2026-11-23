namespace Datahub.TrainingPortal.Application.DTOs.Materials;

public class MaterialFileItemDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
}
