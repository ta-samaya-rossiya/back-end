using Application.DataQuery;
using Application.OpenStreetMap.RegionSearch;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.Regions.Responses;
using WebAPI.Controllers.Shared;
using WebAPI.Controllers.Shared.Responses;
using WebAPI.Controllers.Utility;

namespace WebAPI.Controllers.Regions;

[Route("/api/regions")]
public class RegionsController : ControllerBase
{
    private readonly BaseService<Region> _regionService;
    private readonly BaseService<RegionIndicators> _regionIndicatorsService;
    private readonly BaseService<RegionInLine> _regionInLineService;

    public RegionsController(BaseService<Region> regionService, BaseService<RegionIndicators> regionIndicatorsService,
        BaseService<RegionInLine> regionInLineService)
    {
        _regionService = regionService;
        _regionIndicatorsService = regionIndicatorsService;
        _regionInLineService = regionInLineService;
    }
    
    /// <summary>
    /// Получить полный список всех регионов
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(RegionsFullInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllRegions([FromQuery] Guid? line_id)
    {
        var regions = await _regionService.GetAsync(new DataQueryParams<Region>());
        var ids = regions.Select(x => x.Id).ToArray();
        var indicators = await _regionIndicatorsService.GetAsync(new DataQueryParams<RegionIndicators>
        {
            Expression = i => ids.Contains(i.RegionId)
        });
        var regionInLines = Array.Empty<RegionInLine>();
        if (line_id.HasValue)
        {
            regionInLines = await _regionInLineService.GetAsync(new DataQueryParams<RegionInLine>
            {
                Expression = r => r.LineId == line_id
            });
        }
        
        var response = DtoConvert.RegionsToFullInfoResponse(regions, indicators, regionInLines, HttpContext);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Получить список краткой информации о всех регионах
    /// </summary>
    [HttpGet("brief")]
    [ProducesResponseType(typeof(RegionBriefInfoListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBriefAllRegions()
    {
        var regions = await _regionService.GetAsync(new DataQueryParams<Region>());
        
        var response = new RegionBriefInfoListResponse
        {
            Regions = regions.Select(r => new RegionBriefInfo
            {
                Id = r.Id,
                Title = r.Title
            }).ToArray()
        };
        
        return Ok(response);
    }
    
    /// <summary>
    /// Получить полную информацию о регионе по его id
    /// </summary>
    [HttpGet("{regionId:guid}")]
    [ProducesResponseType(typeof(RegionFullInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegion([FromRoute] Guid regionId, [FromQuery] Guid? line_id)
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

        var region = regions.First();
        
        var indicators = await _regionIndicatorsService.GetAsync(new DataQueryParams<RegionIndicators>
        {
            Expression = i => i.RegionId == region.Id
        });
        
        var regionInLines = Array.Empty<RegionInLine>();
        if (line_id.HasValue)
        {
            regionInLines = await _regionInLineService.GetAsync(new DataQueryParams<RegionInLine>
            {
                Expression = r => r.LineId == line_id
            });
        }
        
        var response = DtoConvert.RegionsToFullInfoResponse(regions, indicators, regionInLines, HttpContext);
        
        return Ok(response.Regions[0]);
    }
    
    /// <summary>
    /// Получить только границу региона по его id
    /// </summary>
    [HttpGet("{regionId:guid}/border")]
    [ProducesResponseType(typeof(BorderResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegionBorder([FromRoute] Guid regionId)
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

        var region = regions.First();
        var border = DtoConvert.ConvertPolygonToLatLon(region.Border);
        
        return Ok(new BorderResponse
        {
            Border = border
        });
    }
    
    /// <summary>
    /// Получить только показатели региона по его id
    /// </summary>
    [HttpGet("{regionId:guid}/indicators")]
    [ProducesResponseType(typeof(IndicatorsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetRegionIndicators([FromRoute] Guid regionId)
    {
        var indicators = await _regionIndicatorsService.GetAsync(new DataQueryParams<RegionIndicators>
        {
            Expression = i => i.RegionId == regionId
        });
        if (indicators.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No region indicators found with provided Region Id ({regionId})"
            });
        }

        var response = DtoConvert.ConvertIndicatorsResponse(indicators.First(), HttpContext);
        
        return Ok(response);
    }
}