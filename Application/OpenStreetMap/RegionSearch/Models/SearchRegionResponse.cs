using Application.OpenStreetMap.SharedModels;

namespace Application.OpenStreetMap.RegionSearch.Models;

public class SearchRegionResponse : BaseOverpassApiResponse
{
    public SearchRegionItem[] Elements { get; set; } = null!;
}