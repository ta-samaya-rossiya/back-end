using System.Runtime.Serialization;
using Application.OpenStreetMap.RussiaImport.Models;
using Application.Services;
using Domain.Entities;
using Newtonsoft.Json;

namespace Application.OpenStreetMap.RussiaImport;

public class RussiaRegionsImporter
{
    private readonly OverpassApiService _overpassApiService;
    private readonly OsmNewRegionsService _newRegionsService;

    private readonly List<int> _newRegions = [
        71022, // Херсонская область
        71973, // Донецкая Народная Республика
        71971 // Луганская Народная Республика
    ];
    
    private readonly List<int> _notRussianRegions = [
        71249, // Черниговская область
    ];
    
    public RussiaRegionsImporter(OverpassApiService overpassApiService, OsmNewRegionsService newRegionsService)
    {
        _overpassApiService = overpassApiService;
        _newRegionsService = newRegionsService;
    }
    
    /// <summary>
    /// Добавить все регионы РФ в базу данных.
    /// </summary>
    /// <returns>Список возникших ошибок в процессе добавления.</returns>
    public async Task<string[]> ImportAllRussiaRegions()
    {
        var query =
            "[out:json][timeout:60];\narea[\"ISO3166-1\"=\"RU\"][admin_level=2]->.russia;\n(\n  relation[\"admin_level\"=\"4\"](area.russia);\n);\nout ids;";
        var responseStr = await _overpassApiService.GetOverpassApiResponse(query);
        var response = JsonConvert.DeserializeObject<GetIdsResponse>(responseStr);
        if (response == null)
        {
            throw new SerializationException("Couldn't deserialize response from OSM to ids model.");
        }
        var ids = response.Elements.Select(el => el.Id).ToList();
        CheckAndAddNewRegions(ids);
        
        var regions = new List<Region>();
        var errors = new List<string>();
        foreach (var id in ids)
        {
            var region = await _newRegionsService.AddNewRegion(id, null);
            if (region.Completed && region.Item != null)
            {
                regions.Add(region.Item!);
            }
            else
            {
                errors.Add($"Region ID({id}). Error: " + (region.Message ?? "Unknown error"));
            }
        }

        return errors.ToArray();
    }

    private void CheckAndAddNewRegions(List<int> ids)
    {
        foreach (var newRegionId in _newRegions)
        {
            if (!ids.Contains(newRegionId))
            {
                ids.Add(newRegionId);
            }
        }

        foreach (var notRussianRegionId in _notRussianRegions)
        {
            if (ids.Contains(notRussianRegionId))
            {
                ids.Remove(notRussianRegionId);
            }
        }
    }
}