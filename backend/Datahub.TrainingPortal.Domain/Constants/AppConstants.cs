namespace Datahub.TrainingPortal.Domain.Constants;

public static class AppConstants
{
    public const long MaxFileSizeBytes = 100 * 1024 * 1024; // 100 MB

    public static readonly string[] AllowedFileExtensions =
    {
        ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx",
        ".mp4", ".mov", ".avi", ".mkv",
        ".zip", ".rar", ".7z",
        ".png", ".jpg", ".jpeg", ".gif", ".svg",
        ".cs", ".ts", ".tsx", ".js", ".jsx", ".py", ".sql", ".json", ".txt", ".md"
    };

    public const string UploadsRootFolder = "Uploads";
}
