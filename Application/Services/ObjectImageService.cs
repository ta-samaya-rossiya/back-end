using Application.Models;
using Domain.Entities;

namespace Application.Services;

public class ObjectImageService
{
    private readonly BaseService<HistoricalObject> _objectService;
    public readonly string ObjectsImagesRelativePath = Path.Combine("images", "objects");

    public ObjectImageService(BaseService<HistoricalObject> objectService)
    {
        _objectService = objectService;
    }
    
    public async Task UpdateObjectImageAsync(HistoricalObject histObject, string webRootPath, FileData? newFileData)
    {
        var folder = Path.Combine(webRootPath, ObjectsImagesRelativePath);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        var currentImagePath = Path.Combine(webRootPath, histObject.ImagePath ?? "");
        if (!string.IsNullOrWhiteSpace(histObject.ImagePath) && File.Exists(currentImagePath))
        {
            File.Delete(currentImagePath);
        }

        if (newFileData == null)
        {
            histObject.ImagePath = null;
        }
        else
        {
            var fileExtension = newFileData.FileName.Split('.')[^1];
            var fileName = $"{histObject.Id}.{fileExtension}";
            var fullPath = Path.Combine(webRootPath, ObjectsImagesRelativePath, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await newFileData.Content.CopyToAsync(stream);
            }

            histObject.ImagePath = Path.Combine(ObjectsImagesRelativePath, fileName);
        }

        await _objectService.UpdateAsync(histObject);
    }
}