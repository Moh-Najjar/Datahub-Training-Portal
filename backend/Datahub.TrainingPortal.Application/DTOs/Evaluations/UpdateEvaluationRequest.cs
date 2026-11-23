namespace Datahub.TrainingPortal.Application.DTOs.Evaluations;

public class UpdateEvaluationRequest
{
    public DateTime EvaluatedAt { get; set; }
    public List<EvaluationCategoryScoreInput> Categories { get; set; } = [];
}
