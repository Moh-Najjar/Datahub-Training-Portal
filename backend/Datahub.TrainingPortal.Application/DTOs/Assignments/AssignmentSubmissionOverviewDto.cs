namespace Datahub.TrainingPortal.Application.DTOs.Assignments;

public class AssignmentSubmissionOverviewDto
{
    public Guid UserId { get; set; }
    public string UserName { get; set; } = string.Empty;
    public AssignmentSubmissionDto? Submission { get; set; }
}
