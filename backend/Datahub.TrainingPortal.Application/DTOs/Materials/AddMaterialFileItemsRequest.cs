using Microsoft.AspNetCore.Http;

namespace Datahub.TrainingPortal.Application.DTOs.Materials;

public class AddMaterialFileItemsRequest
{
    public List<IFormFile> Files { get; set; } = [];
    public List<string>? FileTitles { get; set; }
}
