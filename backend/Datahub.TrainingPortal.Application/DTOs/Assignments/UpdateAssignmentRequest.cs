namespace Datahub.TrainingPortal.Application.DTOs.Assignments;

public class UpdateAssignmentRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public DateTime OpensAt { get; set; }
    public DateTime ClosesAt { get; set; }
}
