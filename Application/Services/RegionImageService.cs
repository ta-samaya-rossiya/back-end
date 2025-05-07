using Domain.Entities;
using Microsoft.AspNetCore.Http;

namespace Application.Services;

public class RegionImageService
{
    private readonly BaseService<RegionIndicators> _regionIndicatorsService;
    public readonly string RegionImagesRelativePath = Path.Combine("images", "regions");

    public RegionImageService(BaseService<RegionIndicators> regionIndicatorsService)
    {
        _regionIndicatorsService = regionIndicatorsService;
    }
    
    public async Task UpdateRegionImageAsync(RegionIndicators indicators, string webRootPath, IFormFile? newFile)
    {
        var folder = Path.Combine(webRootPath, RegionImagesRelativePath);
        if (!Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }
        
        var currentImagePath = Path.Combine(webRootPath, indicators.ImagePath ?? "");
        if (!string.IsNullOrWhiteSpace(indicators.ImagePath) && File.Exists(currentImagePath))
        {
            File.Delete(currentImagePath);
        }

        if (newFile == null)
        {
            indicators.ImagePath = null;
        }
        else
        {
            var fileExtension = newFile.FileName.Split('.')[^1];
            var fileName = $"{indicators.Id}.{fileExtension}";
            var fullPath = Path.Combine(webRootPath, RegionImagesRelativePath, fileName);
            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await newFile.CopyToAsync(stream);
            }

            indicators.ImagePath = Path.Combine(RegionImagesRelativePath, fileName);
        }

        await _regionIndicatorsService.UpdateAsync(indicators);
    }
}