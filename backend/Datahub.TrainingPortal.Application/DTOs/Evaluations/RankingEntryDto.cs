namespace Datahub.TrainingPortal.Application.DTOs.Evaluations;

public class RankingEntryDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public int? LatestTotal { get; set; }
    public int? LatestMaxTotal { get; set; }
    public DateTime? LatestEvaluatedAt { get; set; }
    public int EvaluationCount { get; set; }
}
