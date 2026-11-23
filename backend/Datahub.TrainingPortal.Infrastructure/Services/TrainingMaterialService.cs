using AutoMapper;
using AutoMapper.QueryableExtensions;
using Datahub.TrainingPortal.Application.DTOs.Materials;
using Datahub.TrainingPortal.Application.Exceptions;
using Datahub.TrainingPortal.Application.Interfaces;
using Datahub.TrainingPortal.Domain.Constants;
using Datahub.TrainingPortal.Domain.Entities;
using Datahub.TrainingPortal.Domain.Enums;
using Datahub.TrainingPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace Datahub.TrainingPortal.Infrastructure.Services;

public class TrainingMaterialService(
    AppDbContext db,
    IMapper mapper,
    ICurrentUserService currentUser,
    IFileStorageService fileStorage) : ITrainingMaterialService
{
    private static readonly string[] ViewOnlyRoles = [RoleNames.Intern, RoleNames.Trainee, RoleNames.Employee];

    public async Task<IReadOnlyList<TrainingMaterialDto>> GetForTrainingAsync(Guid trainingId)
    {
        await EnsureViewAccessAsync(trainingId);

        return await db.TrainingMaterials
            .Where(m => m.TrainingId == trainingId)
            .OrderBy(m => m.SortOrder)
            .ThenByDescending(m => m.CreatedAt)
            .ProjectTo<TrainingMaterialDto>(mapper.ConfigurationProvider)
            .ToListAsync();
    }

    public async Task<TrainingMaterialDto> AddFileGroupAsync(Guid trainingId, CreateFileMaterialRequest request)
    {
        var training = await db.Trainings.FirstOrDefaultAsync(t => t.Id == trainingId)
            ?? throw new NotFoundException($"Training '{trainingId}' was not found.");

        EnsureManagedDepartment(training.DepartmentId);

        if (request.Files.Count == 0)
        {
            throw new ArgumentException("At least one file is required.");
        }

        var title = string.IsNullOrWhiteSpace(request.Title) ? request.Files[0].FileName : request.Title;

        var material = new TrainingMaterial
        {
            TrainingId = trainingId,
            Type = MaterialType.File,
            Title = title,
            UploadedByUserId = GetCurrentUserId(),
            SortOrder = await GetNextSortOrderAsync(trainingId)
        };

        db.TrainingMaterials.Add(material);

        for (var i = 0; i < request.Files.Count; i++)
        {
            var itemTitle = request.FileTitles is { Count: > 0 } && i < request.FileTitles.Count
                ? request.FileTitles[i]
                : null;
            material.FileItems.Add(await BuildFileItemAsync(trainingId, material.Id, request.Files[i], itemTitle, i));
        }

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(material.Id);
    }

    public async Task<TrainingMaterialDto> AddTextAsync(Guid trainingId, CreateTextMaterialRequest request)
    {
        var training = await db.Trainings.FirstOrDefaultAsync(t => t.Id == trainingId)
            ?? throw new NotFoundException($"Training '{trainingId}' was not found.");

        EnsureManagedDepartment(training.DepartmentId);

        var material = new TrainingMaterial
        {
            TrainingId = trainingId,
            Type = MaterialType.Text,
            Title = request.Title,
            TextContent = request.Content,
            UploadedByUserId = GetCurrentUserId(),
            SortOrder = await GetNextSortOrderAsync(trainingId)
        };

        db.TrainingMaterials.Add(material);
        await db.SaveChangesAsync();

        return await ProjectSingleAsync(material.Id);
    }

    public async Task<TrainingMaterialDto> AddLinkAsync(Guid trainingId, CreateLinkMaterialRequest request)
    {
        var training = await db.Trainings.FirstOrDefaultAsync(t => t.Id == trainingId)
            ?? throw new NotFoundException($"Training '{trainingId}' was not found.");

        EnsureManagedDepartment(training.DepartmentId);

        var material = new TrainingMaterial
        {
            TrainingId = trainingId,
            Type = MaterialType.Link,
            Title = request.Title,
            UploadedByUserId = GetCurrentUserId(),
            SortOrder = await GetNextSortOrderAsync(trainingId),
            LinkItems = request.Links.Select((link, index) => new MaterialLinkItem
            {
                Title = link.Title,
                Url = link.Url,
                SortOrder = index
            }).ToList()
        };

        db.TrainingMaterials.Add(material);
        await db.SaveChangesAsync();

        return await ProjectSingleAsync(material.Id);
    }

    public async Task<TrainingMaterialDto> UpdateTextAsync(Guid materialId, CreateTextMaterialRequest request)
    {
        var material = await GetOwnedMaterialAsync(materialId, MaterialType.Text);

        material.Title = request.Title;
        material.TextContent = request.Content;
        material.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(material.Id);
    }

    public async Task<TrainingMaterialDto> UpdateLinkAsync(Guid materialId, CreateLinkMaterialRequest request)
    {
        var material = await GetOwnedMaterialAsync(materialId, MaterialType.Link, includeItems: true);

        material.Title = request.Title;
        material.UpdatedAt = DateTime.UtcNow;

        db.MaterialLinkItems.RemoveRange(material.LinkItems);
        db.MaterialLinkItems.AddRange(request.Links.Select((link, index) => new MaterialLinkItem
        {
            MaterialId = material.Id,
            Title = link.Title,
            Url = link.Url,
            SortOrder = index
        }));

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(material.Id);
    }

    public async Task<TrainingMaterialDto> UpdateFileTitleAsync(Guid materialId, UpdateFileMaterialRequest request)
    {
        var material = await GetOwnedMaterialAsync(materialId, MaterialType.File);

        material.Title = request.Title;
        material.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(material.Id);
    }

    public async Task<TrainingMaterialDto> AddFileItemsAsync(Guid materialId, AddMaterialFileItemsRequest request)
    {
        var material = await GetOwnedMaterialAsync(materialId, MaterialType.File, includeItems: true);

        if (request.Files.Count == 0)
        {
            throw new ArgumentException("At least one file is required.");
        }

        var nextSortOrder = material.FileItems.Count == 0 ? 0 : material.FileItems.Max(f => f.SortOrder) + 1;

        var newItems = new List<MaterialFileItem>();
        for (var i = 0; i < request.Files.Count; i++)
        {
            var itemTitle = request.FileTitles is { Count: > 0 } && i < request.FileTitles.Count
                ? request.FileTitles[i]
                : null;
            newItems.Add(await BuildFileItemAsync(material.TrainingId, material.Id, request.Files[i], itemTitle, nextSortOrder + i));
        }

        db.MaterialFileItems.AddRange(newItems);
        material.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return await ProjectSingleAsync(material.Id);
    }

    public async Task<TrainingMaterialDto> UpdateFileItemAsync(Guid materialId, Guid fileItemId, UpdateMaterialFileItemRequest request)
    {
        var material = await GetOwnedMaterialAsync(materialId, MaterialType.File, includeItems: true);

        var item = material.FileItems.FirstOrDefault(f => f.Id == fileItemId)
            ?? throw new NotFoundException($"File '{fileItemId}' was not found.");

        item.Title = request.Title;
        item.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync();

        return await ProjectSingleAsync(material.Id);
    }

    public async Task DeleteFileItemAsync(Guid materialId, Guid fileItemId)
    {
        var material = await GetOwnedMaterialAsync(materialId, MaterialType.File, includeItems: true);

        var item = material.FileItems.FirstOrDefault(f => f.Id == fileItemId)
            ?? throw new NotFoundException($"File '{fileItemId}' was not found.");

        await fileStorage.DeleteAsync(item.BlobPath);

        db.MaterialFileItems.Remove(item);
        await db.SaveChangesAsync();
    }

    public async Task<(Stream Stream, string ContentType, string FileName)> DownloadFileItemAsync(Guid materialId, Guid fileItemId)
    {
        var material = await db.TrainingMaterials
            .Include(m => m.FileItems)
            .FirstOrDefaultAsync(m => m.Id == materialId)
            ?? throw new NotFoundException($"Training material '{materialId}' was not found.");

        await EnsureViewAccessAsync(material.TrainingId);

        var item = material.FileItems.FirstOrDefault(f => f.Id == fileItemId)
            ?? throw new NotFoundException($"File '{fileItemId}' was not found.");

        Stream stream;
        try
        {
            stream = await fileStorage.DownloadAsync(item.BlobPath);
        }
        catch (FileNotFoundException)
        {
            throw new NotFoundException("The file is missing from storage.");
        }

        return (stream, item.ContentType, item.Title is { Length: > 0 } ? item.Title : item.FileName);
    }

    public async Task DeleteAsync(Guid materialId)
    {
        var material = await db.TrainingMaterials
            .Include(m => m.Training)
            .Include(m => m.FileItems)
            .FirstOrDefaultAsync(m => m.Id == materialId)
            ?? throw new NotFoundException($"Training material '{materialId}' was not found.");

        EnsureManagedDepartment(material.Training!.DepartmentId);

        foreach (var item in material.FileItems)
        {
            await fileStorage.DeleteAsync(item.BlobPath);
        }

        db.TrainingMaterials.Remove(material);
        await db.SaveChangesAsync();
    }

    public async Task ReorderAsync(Guid trainingId, ReorderMaterialsRequest request)
    {
        var training = await db.Trainings.FirstOrDefaultAsync(t => t.Id == trainingId)
            ?? throw new NotFoundException($"Training '{trainingId}' was not found.");

        EnsureManagedDepartment(training.DepartmentId);

        var materials = await db.TrainingMaterials
            .Where(m => m.TrainingId == trainingId && request.OrderedIds.Contains(m.Id))
            .ToListAsync();

        var materialsById = materials.ToDictionary(m => m.Id);

        for (var index = 0; index < request.OrderedIds.Count; index++)
        {
            if (!materialsById.TryGetValue(request.OrderedIds[index], out var material))
            {
                throw new NotFoundException($"Training material '{request.OrderedIds[index]}' was not found.");
            }

            material.SortOrder = index;
            material.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();
    }

    private async Task<int> GetNextSortOrderAsync(Guid trainingId)
    {
        var maxSortOrder = await db.TrainingMaterials
            .Where(m => m.TrainingId == trainingId)
            .Select(m => (int?)m.SortOrder)
            .MaxAsync();

        return maxSortOrder.HasValue ? maxSortOrder.Value + 1 : 0;
    }

    private async Task<MaterialFileItem> BuildFileItemAsync(Guid trainingId, Guid materialId, IFormFile file, string? title, int sortOrder)
    {
        if (file.Length == 0)
        {
            throw new ArgumentException("The uploaded file is empty.");
        }

        if (file.Length > AppConstants.MaxFileSizeBytes)
        {
            throw new ArgumentException($"The file exceeds the maximum size of {AppConstants.MaxFileSizeBytes} bytes.");
        }

        var extension = Path.GetExtension(file.FileName);
        if (!AppConstants.AllowedFileExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            throw new ArgumentException($"File extension '{extension}' is not allowed.");
        }

        var storedFileName = $"{Guid.NewGuid()}{extension}";
        var blobPath = $"{trainingId}/{materialId}/{storedFileName}";

        await using (var stream = file.OpenReadStream())
        {
            await fileStorage.UploadAsync(blobPath, stream, file.ContentType);
        }

        return new MaterialFileItem
        {
            MaterialId = materialId,
            Title = title,
            FileName = file.FileName,
            StoredFileName = storedFileName,
            BlobPath = blobPath,
            ContentType = file.ContentType,
            FileSizeBytes = file.Length,
            SortOrder = sortOrder
        };
    }

    private async Task<TrainingMaterial> GetOwnedMaterialAsync(
        Guid materialId,
        MaterialType expectedType,
        bool includeItems = false)
    {
        var query = db.TrainingMaterials.Include(m => m.Training).AsQueryable();

        if (includeItems)
        {
            query = expectedType switch
            {
                MaterialType.File => query.Include(m => m.FileItems),
                MaterialType.Link => query.Include(m => m.LinkItems),
                _ => query
            };
        }

        var material = await query.FirstOrDefaultAsync(m => m.Id == materialId)
            ?? throw new NotFoundException($"Training material '{materialId}' was not found.");

        EnsureManagedDepartment(material.Training!.DepartmentId);

        if (material.Type != expectedType)
        {
            throw new ArgumentException($"Only {expectedType} materials can be updated with this endpoint.");
        }

        return material;
    }

    private async Task<TrainingMaterialDto> ProjectSingleAsync(Guid materialId)
    {
        return await db.TrainingMaterials
            .Where(m => m.Id == materialId)
            .ProjectTo<TrainingMaterialDto>(mapper.ConfigurationProvider)
            .FirstAsync();
    }

    private async Task EnsureViewAccessAsync(Guid trainingId)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin)
        {
            var managedDepartmentId = GetManagedDepartmentId();
            var belongsToDepartment = await db.Trainings.AnyAsync(t =>
                t.Id == trainingId && t.DepartmentId == managedDepartmentId);

            if (!belongsToDepartment)
            {
                throw new ForbiddenException("Department admins can only view materials from their own department.");
            }

            return;
        }

        if (ViewOnlyRoles.Contains(currentUser.Role))
        {
            var userId = GetCurrentUserId();
            var isEnrolled = await db.Enrollments.AnyAsync(e => e.TrainingId == trainingId && e.UserId == userId);

            if (!isEnrolled)
            {
                throw new ForbiddenException("You can only view materials for trainings you are enrolled in.");
            }
        }
    }

    private void EnsureManagedDepartment(Guid departmentId)
    {
        if (currentUser.Role == RoleNames.DepartmentAdmin && currentUser.DepartmentId != departmentId)
        {
            throw new ForbiddenException("Department admins can only manage materials in their own department.");
        }
    }

    private Guid GetManagedDepartmentId()
        => currentUser.DepartmentId
            ?? throw new ForbiddenException("The signed-in department admin is not assigned to a department.");

    private Guid GetCurrentUserId()
        => currentUser.UserId
            ?? throw new UnauthorizedAccessException("A signed-in user is required.");
}
