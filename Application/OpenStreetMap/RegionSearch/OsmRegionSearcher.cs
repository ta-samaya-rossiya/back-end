using System.Text;
using Application.OpenStreetMap.RegionSearch.Models;
using Application.Services.Http;
using Application.Services.Http.Enums;
using Application.Services.Http.Models;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;
using Newtonsoft.Json;

namespace Application.OpenStreetMap.RegionSearch;

public class OsmRegionSearcher
{
    private readonly IHttpService _httpService;
    private readonly OverpassApiService _overpassApiService;

    public OsmRegionSearcher(IHttpService httpService, OverpassApiService overpassApiService)
    {
        _httpService = httpService;
        _overpassApiService = overpassApiService;
    }
    
    public async Task<ServiceActionResult<SearchRegionResponse>> SearchForRegionsAsync(string str, params int[] adminLevels)
    {
        var request = $"[out:json][timeout:25];" +
                      $"(";
        foreach (var adminLevel in adminLevels)
        {
            request += $"  relation[\"admin_level\"=\"{adminLevel}\"][\"name\"~\"{str}\",i];";
        }
        request += $");" +
                   $"out tags;";
        
        var response = await _overpassApiService.GetOverpassApiResponse(request);

        var result = JsonConvert.DeserializeObject<SearchRegionResponse>(response);
        return new ServiceActionResult<SearchRegionResponse>
        {
            Item = result,
            Completed = true,
            Message = string.Empty
        };
    }
    
    public async Task<ServiceActionResult<Geometry>> GetRegionGeometryAsync(int regionId)
    {
        var osmData = await GetOsmContentAsync(regionId);

        var geojson = await OsmToGeoJsonConverter.OsmToGeoJsonAsync(osmData.Item!);
        var simplifiedGeom = await GeometrySimplifier.SimplifyToPercentageAsync(geojson);
        return new ServiceActionResult<Geometry>
        {
            Item = simplifiedGeom,
            Completed = true,
            Message = string.Empty
        };
    }

    public async Task<ServiceActionResult<string>> GetRegionTitleAsync(int regionId)
    {
        var response = await _overpassApiService.GetOverpassApiResponse($"[out:json];relation({regionId});out tags;");
        var result = JsonConvert.DeserializeObject<SearchRegionResponse>(response);
        
        return new ServiceActionResult<string>
        {
            Item = result!.Elements[0].Name,
            Completed = true,
            Message = string.Empty
        };
    }
    
    private async Task<ServiceActionResult<string>> GetOsmContentAsync(int regionId, bool useXml = false)
    {
        var query = $"[out:{(useXml ? "xml" : "json")}];relation({regionId});out geom;";
        var result = await _overpassApiService.GetOverpassApiResponse(query);
        
        return new ServiceActionResult<string>
        {
            Item = result,
            Completed = true,
            Message = string.Empty
        };
    }
}