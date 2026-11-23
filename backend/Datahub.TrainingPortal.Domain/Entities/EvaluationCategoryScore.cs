using Datahub.TrainingPortal.Domain.Common;

namespace Datahub.TrainingPortal.Domain.Entities;

public class EvaluationCategoryScore : AuditableEntity
{
    public Guid EvaluationId { get; set; }
    public Evaluation? Evaluation { get; set; }

    public string CategoryName { get; set; } = string.Empty;
    public string Criteria { get; set; } = string.Empty;
    public int Score { get; set; }
    public int SortOrder { get; set; }
}
