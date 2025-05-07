using Application.Models;
using Domain.Entities;

namespace Application.Services;

public class MarkerImageService
{
    private readonly BaseService<HistoricalLine> _lineService;
    public readonly string MarkerImagesRelativePath = Path.Combine("images", "markers");
    public const string MarkerDefaultImageFile = "marker-default.png";

    public MarkerImageService(BaseService<HistoricalLine> lineService)
    {
        _lineService = lineService;
    }
    
    public async Task UpdateLineMarkerImageAsync(HistoricalLine line, string webRootPath, FileData newFileData)
    {
        var folder = Path.Combine(webRootPath, MarkerImagesRelativePath);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        var currentImagePath = Path.Combine(webRootPath, line.MarkerImagePath ?? "");
        if (!string.IsNullOrWhiteSpace(line.MarkerImagePath) && !line.MarkerImagePath.Contains(MarkerDefaultImageFile) 
                                                             && File.Exists(currentImagePath))
        {
            File.Delete(currentImagePath);
        }
        
        var fileExtension = newFileData.FileName.Split('.')[^1];
        var fileName = $"{line.Id}.{fileExtension}";
        var fullPath = Path.Combine(webRootPath, MarkerImagesRelativePath, fileName);
        await using (var stream = new FileStream(fullPath, FileMode.Create))
        {
            await newFileData.Content.CopyToAsync(stream);
        }
        line.MarkerImagePath = Path.Combine(MarkerImagesRelativePath, fileName);
        
        await _lineService.UpdateAsync(line);
    }
    
    public string GetDefaultImageForMarker()
    {
        return Path.Combine(MarkerImagesRelativePath, MarkerDefaultImageFile);
    }
}