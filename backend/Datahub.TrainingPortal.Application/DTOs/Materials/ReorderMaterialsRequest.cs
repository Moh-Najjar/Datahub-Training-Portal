namespace Datahub.TrainingPortal.Application.DTOs.Materials;

public class ReorderMaterialsRequest
{
    public List<Guid> OrderedIds { get; set; } = [];
}
