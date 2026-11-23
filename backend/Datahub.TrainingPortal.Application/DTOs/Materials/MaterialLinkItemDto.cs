namespace Datahub.TrainingPortal.Application.DTOs.Materials;

public class MaterialLinkItemDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public string Url { get; set; } = string.Empty;
}
