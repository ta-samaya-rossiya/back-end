using System.Runtime.Serialization;
using Application.OpenStreetMap.RussiaImport.Models;
using Application.Services;
using Domain.Entities;
using Newtonsoft.Json;
using Serilog;

namespace Application.OpenStreetMap.RussiaImport;

public class RussiaRegionsImporter
{
    private readonly OverpassApiService _overpassApiService;
    private readonly OsmNewRegionsService _newRegionsService;

    private readonly List<int> _newRegions = [
        71022, // Херсонская область
        71973, // Донецкая Народная Республика
        71971, // Луганская Народная Республика
        72639, // Крым
        1574364 // Севастополь
    ];
    
    private readonly List<int> _notRussianRegions = [
        71249, // Черниговская область
        1999428, // Pohjois-Karjala
        17518688, // Финляндия
        3795586, // Повторяющийсая Крым
        3788485 // Повторяющийся Севастополь
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
        Log.Information("[Russia import] Запущено добавление всех регионов РФ");
        var query =
            "[out:json][timeout:60];\nrel[\"admin_level\"=\"2\"][\"ISO3166-1\"=\"RU\"];\nmap_to_area -> .russia;\nrelation[\"admin_level\"=\"4\"](area.russia);\nout ids;";
        var responseStr = await _overpassApiService.GetOverpassApiResponse(query);
        var response = JsonConvert.DeserializeObject<GetIdsResponse>(responseStr);
        if (response == null)
        {
            throw new SerializationException("Couldn't deserialize response from OSM to ids model.");
        }
        var ids = response.Elements.Select(el => el.Id).ToList();
        CheckAndAddNewRegions(ids);
        Log.Information("[Russia import] Всего найдено {count} регионов", ids.Count);
        var regions = new List<Region>();
        var errors = new List<string>();
        var countAdded = 1;
        foreach (var id in ids)
        {
            var region = await _newRegionsService.AddNewRegion(id, null);
            if (region.Completed && region.Item != null)
            {
                regions.Add(region.Item!);
                Log.Information("[Russia import] ({countAdded}/{totalCount}) Добавлен регион {title} с OsmId = {id}",
                    countAdded, ids.Count, region.Item.Title, id);
            }
            else
            {
                errors.Add($"Region ID({id}). Error: " + (region.Message ?? "Unknown error"));
                Log.Error("[Russia import] ({countAdded}/{totalCount}) Ошибка при добавлении региона с OsmId = {id}", 
                    countAdded, ids.Count, id);
            }

            countAdded++;
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