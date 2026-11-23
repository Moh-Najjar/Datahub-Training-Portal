namespace Datahub.TrainingPortal.Application.DTOs.Evaluations;

public class EvaluationCategoryScoreDto
{
    public Guid Id { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string Criteria { get; set; } = string.Empty;
    public int Score { get; set; }
}
