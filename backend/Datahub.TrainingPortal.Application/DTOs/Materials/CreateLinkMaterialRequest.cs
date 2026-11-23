namespace Datahub.TrainingPortal.Application.DTOs.Materials;

public class CreateLinkMaterialRequest
{
    public string Title { get; set; } = string.Empty;
    public List<LinkItemInput> Links { get; set; } = [];
}
