namespace Datahub.TrainingPortal.Application.DTOs.Departments;

public class DepartmentDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public int UserCount { get; set; }
    public int TrainingCount { get; set; }
}
