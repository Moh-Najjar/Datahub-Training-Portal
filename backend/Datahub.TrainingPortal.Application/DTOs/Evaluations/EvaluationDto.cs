namespace Datahub.TrainingPortal.Application.DTOs.Evaluations;

public class EvaluationDto
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public Guid EvaluatedByUserId { get; set; }
    public string? EvaluatedByUserName { get; set; }
    public Guid? EnrollmentId { get; set; }
    public Guid? TrainingId { get; set; }
    public string? TrainingTitle { get; set; }
    public string? DepartmentName { get; set; }
    public DateTime EvaluatedAt { get; set; }
    public int Total { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<EvaluationCategoryScoreDto> Categories { get; set; } = [];
}
