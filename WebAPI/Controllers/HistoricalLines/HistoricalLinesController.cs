using Application.DataQuery;
using Application.Services;
using Domain.Entities;
using Microsoft.AspNetCore.Mvc;
using WebAPI.Controllers.HistoricalLines.Responses;
using WebAPI.Controllers.Shared.Responses;
using WebAPI.Controllers.Utility;

namespace WebAPI.Controllers.HistoricalLines;

[Route("api/historical-lines")]
public class HistoricalLinesController : ControllerBase
{
    private readonly BaseService<HistoricalLine> _historicalLineService;
    private readonly BaseService<HistoricalObject> _objectsService;
    private readonly BaseService<Region> _regionService;
    private readonly BaseService<RegionInLine> _regionInLineService;

    public HistoricalLinesController(BaseService<HistoricalLine> historicalLineService, BaseService<HistoricalObject> objectsService,
        BaseService<Region> regionService, BaseService<RegionInLine> regionInLineService)
    {
        _historicalLineService = historicalLineService;
        _objectsService = objectsService;
        _regionService = regionService;
        _regionInLineService = regionInLineService;
    }
    
    /// <summary>
    /// Получить краткую информацию по всем историческим линиям
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(BriefLineInfosResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool? active = false)
    {
        active ??= false;
        var lines = await _historicalLineService.GetAsync(new DataQueryParams<HistoricalLine>
        {
            Expression = l => !active.Value || l.IsActive,
        });

        var response = new BriefLineInfosResponse
        {
            Lines = lines.Select(l => new BriefLineInfo
            {
                Id = l.Id,
                Title = l.Title
            }).ToArray()
        };

        return Ok(response);
    }
    
    /// <summary>
    /// Получить полную информацию по исторической линии по id
    /// </summary>
    [HttpGet("{lineId:guid}")]
    [ProducesResponseType(typeof(FullLineInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetFullLineInfo([FromRoute] Guid lineId)
    {
        var lines = await _historicalLineService.GetAsync(new DataQueryParams<HistoricalLine>
        {
            Expression = l => l.Id == lineId,
        });
        if (lines.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No historical lines found with provided id ({lineId})"
            });
        }
        var line = lines.First();
        
        var objects = await _objectsService.GetAsync(new DataQueryParams<HistoricalObject>
        {
            Expression = ob => ob.LineId == line.Id
        });
        var regionsInLine = await _regionInLineService.GetAsync(new DataQueryParams<RegionInLine>
        {
            Expression = r => r.LineId == line.Id,
            IncludeParams = new IncludeParams<RegionInLine>
            {
                IncludeProperties = [r => r.Region]
            }
        });
        var regions = regionsInLine.Select(r => r.Region).ToArray();
        
        var response = DtoConvert.GetFullLineResponse(line, objects, regions, HttpContext);

        return Ok(response);
    }
    
    
    /// <summary>
    /// Получить полную информацию по исторической линии по id
    /// </summary>
    [HttpGet("objects/{objectId:guid}")]
    [ProducesResponseType(typeof(HistoricalObjectInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetObjectInfo([FromRoute] Guid objectId)
    {
        var objects = await _objectsService.GetAsync(new DataQueryParams<HistoricalObject>
        {
            Expression = ob => ob.Id == objectId
        });
        if (objects.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No historical object found with provided id ({objectId})"
            });
        }

        var obj = objects.First();
        
        var response = new HistoricalObjectInfoResponse
        {
            Id = obj.Id,
            Title = obj.Title,
            Image = obj.ImagePath == null ? null : DtoConvert.ConvertImagePathToUrl(obj.ImagePath, HttpContext),
            Description = obj.Description,
            VideoUrl = obj.VideoUrl
        };

        return Ok(response);
    }
}