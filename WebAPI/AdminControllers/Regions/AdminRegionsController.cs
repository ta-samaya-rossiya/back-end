using Application.OpenStreetMap.RegionSearch;
using Application.Services;
using Microsoft.AspNetCore.Mvc;
using NetTopologySuite.IO;
using WebAPI.AdminControllers.Regions.Requests;
using WebAPI.Controllers.Regions.Responses;
using WebAPI.Controllers.Shared;

namespace WebAPI.AdminControllers.Regions;

[Route("/api/admin/regions")]
public class AdminRegionsController : ControllerBase
{
    private readonly OsmNewRegionsService _osmNewRegionsService;

    public AdminRegionsController(OsmNewRegionsService osmNewRegionsService)
    {
        _osmNewRegionsService = osmNewRegionsService;
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
        
        // Запрос для получения геометрии (заменить 79379)
        // https://overpass-api.de/api/interpreter?data=[out:json][timeout:25];relation(79379);out%20geom;
    }
    
    /// <summary>
    /// Добавление нового региона в историческую линию по его id с OSM
    /// </summary>
    [HttpPost()]
    [ProducesResponseType(typeof(RegionBasicInfoResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> AddNewRegionToHistoricalLine([FromBody] AddNewRegionRequest dto)
    {
        var res = await _osmNewRegionsService.AddNewRegion(dto.RegionId, dto.LineId);
        
        if (res is { Completed: true, Item: not null })
        {
            return Ok(new RegionBasicInfoResponse
            {
                Completed = true,
                Message = dto.LineId.HasValue ? "Successfully added new region to historical line." : "Successfully added new region as Russian.",
                Id = Guid.NewGuid(),
                Title = res.Item.Title
            });
        }
        return SharedResponses.FailedRequest(res.Message);
    }
}