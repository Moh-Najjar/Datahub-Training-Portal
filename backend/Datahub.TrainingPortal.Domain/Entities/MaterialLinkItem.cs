using Datahub.TrainingPortal.Domain.Common;

namespace Datahub.TrainingPortal.Domain.Entities;

public class MaterialLinkItem : AuditableEntity
{
    public Guid MaterialId { get; set; }
    public TrainingMaterial? Material { get; set; }

    public string? Title { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }
}
