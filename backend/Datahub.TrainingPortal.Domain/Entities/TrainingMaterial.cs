using Datahub.TrainingPortal.Domain.Common;
using Datahub.TrainingPortal.Domain.Enums;

namespace Datahub.TrainingPortal.Domain.Entities;

public class TrainingMaterial : AuditableEntity
{
    public Guid TrainingId { get; set; }
    public Training? Training { get; set; }

    public MaterialType Type { get; set; }
    public string Title { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public string? TextContent { get; set; }

    public Guid UploadedByUserId { get; set; }
    public User? UploadedByUser { get; set; }

    public ICollection<MaterialFileItem> FileItems { get; set; } = new List<MaterialFileItem>();
    public ICollection<MaterialLinkItem> LinkItems { get; set; } = new List<MaterialLinkItem>();
}
