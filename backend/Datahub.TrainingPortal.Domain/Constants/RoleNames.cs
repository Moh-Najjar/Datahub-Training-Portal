namespace Datahub.TrainingPortal.Domain.Constants;

public static class RoleNames
{
    public const string SuperAdmin = "SuperAdmin";
    public const string DepartmentAdmin = "DepartmentAdmin";
    public const string Trainee = "Trainee";
    public const string Employee = "Employee";
    public const string Intern = "Intern";

    public static readonly string[] All =
    {
        SuperAdmin, DepartmentAdmin, Trainee, Employee, Intern
    };
}

public static class DepartmentNames
{
    public const string DataEngineering = "Data Engineering";
    public const string ArtificialIntelligence = "Artificial Intelligence";
    public const string SoftwareDevelopment = "Software Development";
    public const string PowerBi = "Power BI";
    public const string BusinessAnalysisAndBrd = "Business Analysis and BRD";
    public const string Infrastructure = "Infrastructure";
    public const string Cloud = "Cloud";
}

public static class PolicyNames
{
    public const string SuperAdminOnly = "SuperAdminOnly";
    public const string DepartmentManagement = "DepartmentManagement";
    public const string TrainingAuthoring = "TrainingAuthoring";
    public const string SameDepartment = "SameDepartment";
}
