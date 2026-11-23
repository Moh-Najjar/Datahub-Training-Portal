using Datahub.TrainingPortal.Domain.Common;

namespace Datahub.TrainingPortal.Domain.Entities;

public class MaterialFileItem : AuditableEntity
{
    public Guid MaterialId { get; set; }
    public TrainingMaterial? Material { get; set; }

    public string? Title { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string BlobPath { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public int SortOrder { get; set; }
}
