using Application.DataQuery;
using Application.Models;
using Application.Services;
using Application.Services.Colors;
using Domain.Entities;
using Domain.Enums;
using Microsoft.AspNetCore.Mvc;
using WebAPI.AdminControllers.HistoricalLines.Requests;
using WebAPI.AdminControllers.Regions.Requests;
using WebAPI.Controllers.HistoricalLines.Responses;
using WebAPI.Controllers.Shared;
using WebAPI.Controllers.Shared.Responses;
using WebAPI.Controllers.Utility;

namespace WebAPI.AdminControllers.HistoricalLines;

[Route("/api/admin/historical-lines")]
public class AdminHistoricalLinesController : Controller
{
    private readonly BaseService<HistoricalLine> _lineService;
    private readonly BaseService<RegionInLine> _regionsInLinesService;
    private readonly MarkerImageService _markerImageService;
    private readonly IWebHostEnvironment _env;

    public AdminHistoricalLinesController(BaseService<HistoricalLine> lineService, BaseService<RegionInLine> regionsInLinesService,
        MarkerImageService markerImageService, IWebHostEnvironment env)
    {
        _lineService = lineService;
        _regionsInLinesService = regionsInLinesService;
        _markerImageService = markerImageService;
        _env = env;
    }
    
    /// <summary>
    /// Добавление новой исторической линии
    /// </summary>
    [HttpPost()]
    [ProducesResponseType(typeof(FullLineInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddNewHistoricalLine()
    {
        var line = new HistoricalLine
        {
            Id = Guid.NewGuid(),
            Title = "Новая историческая линия",
            MarkerImagePath = _markerImageService.GetDefaultImageForMarker(),
            LineColor = ColorService.GetRandomColorForLine(),
            LineStyle = LineStyle.Dashed,
            MarkerLegend = "Исторический объект",
            IsActive = false,
            LastUpdatedAt = DateTime.Now.ToUniversalTime()
        };
        await _lineService.CreateAsync(line);
        
        var response = DtoConvert.GetFullLineResponse(line, [], [], [], HttpContext);
        
        return Ok(response);
    }
    
    /// <summary>
    /// Обновление исторической линии
    /// </summary>
    [HttpPut("{lineId:guid}")]
    [ProducesResponseType(typeof(FullLineInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateHistoricalLine([FromRoute] Guid lineId, [FromBody] UpdateHistoricalLineRequest dto)
    {
        var lines = await _lineService.GetAsync(new DataQueryParams<HistoricalLine>
        {
            Expression = l => l.Id == lineId
        });
        if (lines.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No historical lines found with provided id ({lineId})"
            });
        }

        var line = lines[0];
        line.Title = dto.Title;
        line.LineStyle = LineStyleExtensions.ParseFromString(dto.LineStyle);
        line.LineColor = dto.LineColor;
        line.MarkerLegend = dto.MarkerLegend;
        line.IsActive = dto.IsActive;
        await _lineService.UpdateAsync(line);

        var currentRegionInLines = await _regionsInLinesService.GetAsync(new DataQueryParams<RegionInLine>
        {
            Expression = r => r.LineId == line.Id
        });
        foreach (var regionInLine in currentRegionInLines)
        {
            regionInLine.IsActive = false;
            await _regionsInLinesService.UpdateAsync(regionInLine);
        }
        
        foreach (var idStr in dto.ActiveRegionIds)
        {
            var id = new Guid(idStr);
            var regionInLine = new RegionInLine
            {
                Id = Guid.NewGuid(),
                LineId = line.Id,
                RegionId = id,
                IsActive = true
            };
            if (currentRegionInLines.Any(r => r.RegionId == id))
            {
                regionInLine = currentRegionInLines.First(r => r.RegionId == id);
                regionInLine.IsActive = true;
                await _regionsInLinesService.UpdateAsync(regionInLine);
            }
            else
            {
                await _regionsInLinesService.CreateAsync(regionInLine);
            }
        }
        
        return Ok(new BaseStatusResponse
        {
            Completed = true,
            Message = "Historical line updated"
        });
    }
    
    
    /// <summary>
    /// Обновить изображение маркера в исторической линии
    /// </summary>
    [HttpPost("{lineId:guid}/marker-image")]
    [ProducesResponseType(typeof(BaseStatusWithImageResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateMarkerImage([FromRoute] Guid lineId, IFormFile? file)
    {
        if (file == null)
        {
            return SharedResponses.FailedRequest("Request doesn't contain file.");
        }

        var lines = await _lineService.GetAsync(new DataQueryParams<HistoricalLine>()
        {
            Expression = r => r.Id == lineId
        });
        if (lines.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No regions found with provided Region Id ({lineId})"
            });
        }
        var line = lines[0];
        var fileData = new FileData
        {
            FileName = file.FileName,
            ContentType = file.ContentType,
            Content = file.OpenReadStream()
        };
        await _markerImageService.UpdateLineMarkerImageAsync(line, _env.WebRootPath, fileData);

        return Ok(new BaseStatusWithImageResponse
        {
            Image = DtoConvert.ConvertImagePathToUrl(line.MarkerImagePath, HttpContext),
            Completed = true,
            Message = "Marker image updated"
        });
    }
    
    /// <summary>
    /// Удалить историческую линию
    /// </summary>
    [HttpDelete("{lineId:guid}")]
    [ProducesResponseType(typeof(FullLineInfoResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(BaseStatusResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteHistoricalLine([FromRoute] Guid lineId)
    {
        var lines = await _lineService.GetAsync(new DataQueryParams<HistoricalLine>
        {
            Expression = l => l.Id == lineId
        });
        if (lines.Length == 0)
        {
            return NotFound(new BaseStatusResponse
            {
                Completed = true,
                Message = $"No historical lines found with provided id ({lineId})"
            });
        }

        var line = lines[0];
        var currentRegionInLines = await _regionsInLinesService.GetAsync(new DataQueryParams<RegionInLine>
        {
            Expression = r => r.LineId == line.Id
        });

        await _regionsInLinesService.RemoveRangeAsync(currentRegionInLines);
        
        var completed = await _lineService.TryRemoveAsync(line.Id);
        
        return Ok(new BaseStatusResponse
        {
            Completed = completed,
            Message = completed ? "Historical line deleted" : $"Exception occured while deleting historical line with id ({lineId})"
        });
    }
}