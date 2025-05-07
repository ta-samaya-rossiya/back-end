using Application.DataQuery;
using Application.OpenStreetMap.RegionSearch;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using WebAPI.AdminControllers.Regions.Requests;
using WebAPI.Controllers.Regions.Requests;
using WebAPI.Controllers.Regions.Responses;
using WebAPI.Controllers.Shared;
using WebAPI.Controllers.Shared.Responses;
using WebAPI.Controllers.Utility;

namespace WebAPI.AdminControllers.Regions;

[Route("/api/admin/regions")]
public class AdminRegionsController : ControllerBase
{
    private readonly OsmNewRegionsService _osmNewRegionsService;
    private readonly BaseService<RegionIndicators> _regionIndicatorsService;
    private readonly BaseService<RegionInLine> _regionInLineService;
    private readonly BaseService<Region> _regionService;
    private readonly IWebHostEnvironment _env;
    private readonly RegionImageService _regionImageService;

    public AdminRegionsController(OsmNewRegionsService osmNewRegionsService, BaseService<RegionIndicators> regionIndicatorsService,
        BaseService<RegionInLine> regionInLineService, BaseService<Region> regionService, IWebHostEnvironment env,
        RegionImageService regionImageService)
    {
        _osmNewRegionsService = osmNewRegionsService;
        _regionIndicatorsService = regionIndicatorsService;
        _regionInLineService = regionInLineService;
        _regionService = regionService;
        _env = env;
        _regionImageService = regionImageService;
    }
    
    /// <summary>
    /// Поиск по существующим регионам из БД OpenStreetMap
    /// </summary>
    [HttpGet("search")]
    [ProducesResponseType(typeof(OsmSearchResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchForRegions([FromQuery] string query)
    {
        if (string.IsNullOrWhiteSpace(query))
        {
            return SharedResponses.FailedRequest("Некорректный Query-параметр \"query\".");
        }
        var res = await _osmNewRegionsService.SearchForRegionsAsync(query, 2, 4);
        if (res is { Completed: true, Item: not null })
        {
            return Ok(new OsmSearchResponse
            {
                Results = res.Item.Elements.Select(i => new OsmRegionBasicInfo
                {
                    Title = i.Name,
                    Id = i.Id
                }).ToArray(),
                Completed = true,
                Message = ""
            });
        }
        return SharedResponses.FailedRequest(res.Message!);
    }
    
    /// <summary>
    /// Добавление нового региона в историческую линию по его id с OSM
    /// </summary>
    [HttpPost()]
    [ProducesResponseType(typeof(RegionFullInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddNewRegionToHistoricalLine([FromBody] AddNewRegionRequest dto)
    {
        var res = await _osmNewRegionsService.AddNewRegion(dto.RegionId, dto.LineId);
        
        if (res is { Completed: true, Item: not null })
        {
            var region = res.Item;
            
            var indicators = await _regionIndicatorsService.GetAsync(new DataQueryParams<RegionIndicators>
            {
                Expression = i => i.RegionId == region.Id
            });
        
            var regionInLines = Array.Empty<RegionInLine>();
            if (dto.LineId.HasValue)
            {
                regionInLines = await _regionInLineService.GetAsync(new DataQueryParams<RegionInLine>
                {
                    Expression = r => r.LineId == dto.LineId
                });
            }
        
            var response = DtoConvert.RegionsToFullInfoResponse([region], indicators, regionInLines, HttpContext);
        
            return Ok(response.Regions[0]);
        }
        return SharedResponses.FailedRequest(res.Message);
    }
    
    /// <summary>
    /// Обновить информацию о регионе
    /// </summary>
    [HttpPut("{regionId:guid}")]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRegionInfo([FromRoute] Guid regionId, [FromBody] UpdateRegionInfoRequest dto)
    {
        var regions = await _regionService.GetAsync(new DataQueryParams<Region>
        {
            Expression = r => r.Id == regionId
        });
        if (regions.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No regions found with provided Region Id ({regionId})"
            });
        }

        var region = regions[0];
        var indicatorsRes = await _regionIndicatorsService.GetAsync(new DataQueryParams<RegionIndicators>
        {
            Expression = ind => ind.RegionId == region.Id
        });
        var indicators = indicatorsRes[0];
        region.Title = dto.Title;
        region.DisplayTitle = dto.DisplayTitle.Text;
        region.FillColor = dto.Color;
        region.ShowIndicators = dto.ShowIndicators;
        region.DisplayTitlePosition =
            DtoConvert.ConvertLatLonToPoint(dto.DisplayTitle.Position[0], dto.DisplayTitle.Position[1]);
        region.DisplayTitleFontSize = dto.DisplayTitle.FontSize;
        await _regionService.UpdateAsync(region);
        
        indicators.Excursions = dto.Indicators.Excursions;
        indicators.Partners = dto.Indicators.Partners;
        indicators.Participants = dto.Indicators.Participants;
        await _regionIndicatorsService.UpdateAsync(indicators);
        
        return Ok(new BaseStatusResponse
        {
            Completed = true,
            Message = "Region updated"
        });
    }
    
    /// <summary>
    /// Удалить регион из базы данных
    /// </summary>
    [HttpDelete("{regionId:guid}")]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> DeleteRegion([FromRoute] Guid regionId)
    {
        var regionInLines = await _regionInLineService.GetAsync(new DataQueryParams<RegionInLine>
        {
            Expression = r => r.RegionId == regionId
        });
        await _regionInLineService.RemoveRangeAsync(regionInLines);
        
        var completed = await _regionService.TryRemoveAsync(regionId);
        
        return Ok(new BaseStatusResponse
        {
            Completed = completed,
            Message = completed ? "Region deleted" : $"Region with provided id ({regionId}) not found"
        });
    }
    
    /// <summary>
    /// Обновить изображение герба региона
    /// </summary>
    [HttpPost("{regionId:guid}/image")]
    [ProducesResponseType(typeof(BaseStatusWithImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRegionImage([FromRoute] Guid regionId, IFormFile? file)
    {
        if (file == null)
        {
            return SharedResponses.FailedRequest("Request doesn't contain file.");
        }

        var indicatorsRes = await _regionIndicatorsService.GetAsync(new DataQueryParams<RegionIndicators>()
        {
            Expression = r => r.RegionId == regionId
        });
        if (indicatorsRes.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No regions found with provided Region Id ({regionId})"
            });
        }
        var indicators = indicatorsRes[0];
        await _regionImageService.UpdateRegionImageAsync(indicators, _env.WebRootPath, file);

        return Ok(new BaseStatusWithImageResponse
        {
            Image = DtoConvert.ConvertImagePathToUrl(indicators.ImagePath, HttpContext),
            Completed = true,
            Message = "Region image updated"
        });
    }
    
    /// <summary>
    /// Обновить изображение герба региона
    /// </summary>
    [HttpDelete("{regionId:guid}/image")]
    [ProducesResponseType(typeof(BaseStatusWithImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteRegionImage([FromRoute] Guid regionId)
    {
        var indicatorsRes = await _regionIndicatorsService.GetAsync(new DataQueryParams<RegionIndicators>()
        {
            Expression = r => r.RegionId == regionId
        });
        if (indicatorsRes.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No regions found with provided Region Id ({regionId})"
            });
        }
        var indicators = indicatorsRes[0];
        await _regionImageService.UpdateRegionImageAsync(indicators, _env.WebRootPath, null);

        return Ok(new BaseStatusWithImageResponse
        {
            Image = null!,
            Completed = true,
            Message = "Region image deleted"
        });
    }
}