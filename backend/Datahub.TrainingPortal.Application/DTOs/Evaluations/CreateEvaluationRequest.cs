namespace Datahub.TrainingPortal.Application.DTOs.Evaluations;

public class CreateEvaluationRequest
{
    public Guid? EnrollmentId { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public List<EvaluationCategoryScoreInput> Categories { get; set; } = [];
}
