using Microsoft.AspNetCore.Http;

namespace Datahub.TrainingPortal.Application.DTOs.Materials;

public class CreateFileMaterialRequest
{
    public string? Title { get; set; }
    public List<IFormFile> Files { get; set; } = [];
    public List<string>? FileTitles { get; set; }
}
