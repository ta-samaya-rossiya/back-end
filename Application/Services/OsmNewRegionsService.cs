using Application.OpenStreetMap.RegionSearch;
using Application.OpenStreetMap.RegionSearch.Models;
using Application.Services.Colors;
using Application.Services.Http.Models;
using Domain.Entities;
using NetTopologySuite.Geometries;

namespace Application.Services;

public class OsmNewRegionsService
{
    private readonly OsmRegionSearcher _osmRegionSearcher;
    private readonly BaseService<Region> _regionService;
    private readonly BaseService<RegionIndicators> _regionIndicatorsService;
    private readonly BaseService<RegionInLine> _regionInLineService;

    public OsmNewRegionsService(OsmRegionSearcher osmRegionSearcher, BaseService<Region> regionService,
        BaseService<RegionIndicators> regionIndicatorsService, BaseService<RegionInLine> regionInLineService)
    {
        _osmRegionSearcher = osmRegionSearcher;
        _regionService = regionService;
        _regionIndicatorsService = regionIndicatorsService;
        _regionInLineService = regionInLineService;
    }
    
    public async Task<ServiceActionResult<Region>> AddNewRegion(int osmRegionId, Guid? lineId)
    {
        var res = await _osmRegionSearcher.GetRegionGeometryAsync(osmRegionId);
        var title = await _osmRegionSearcher.GetRegionTitleAsync(osmRegionId);
        if (!res.Completed)
        {
            return new ServiceActionResult<Region>
            {
                Item = null,
                Completed = false,
                Message = res.Message
            };
        }

        MultiPolygon geometry;
        if (res.Item is Polygon polygon)
        {
            geometry = new MultiPolygon(new[] { polygon });
        }
        else if (res.Item is MultiPolygon multiPolygon)
        {
            geometry = multiPolygon;
        }
        else
        {
            throw new Exception($"Invalid type of geometry \"{res.Item!.GetType()}\"");
        }
        
        var region = new Region
        {
            Id = Guid.NewGuid(),
            Title = title.Item ?? "Новый регион",
            Border = geometry,
            DisplayTitle = title.Item ?? "Новый регион",
            DisplayTitleFontSize = 0,
            DisplayTitlePosition = geometry.Centroid,
            ShowDisplayTitle = true,
            FillColor = ColorService.GetRandomColorForRegion(),
            ShowIndicators = true,
            IsRussia = !lineId.HasValue
        };
        var indicators = new RegionIndicators
        {
            Id = Guid.NewGuid(),
            RegionId = region.Id,
            ImagePath = null,
            Excursions = 0,
            Partners = 0,
            Participants = 0
        };
        await _regionService.CreateAsync(region);
        await _regionIndicatorsService.CreateAsync(indicators);
        if (lineId.HasValue)
        {
            await _regionInLineService.CreateAsync(new RegionInLine
            {
                Id = Guid.NewGuid(),
                LineId = lineId.Value,
                RegionId = region.Id,
                IsActive = true
            });
        }
        return new ServiceActionResult<Region>
        {
            Item = region,
            Completed = true,
            Message = string.Empty
        };
    }
    
    public async Task<ServiceActionResult<SearchRegionResponse>> SearchForRegionsAsync(string str, params int[] adminLevels)
    {
        return await _osmRegionSearcher.SearchForRegionsAsync(str, adminLevels);
    }
}