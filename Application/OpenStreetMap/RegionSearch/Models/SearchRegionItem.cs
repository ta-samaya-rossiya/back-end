﻿namespace Application.OpenStreetMap.RegionSearch.Models;

public class SearchRegionItem
{
    public long Id { get; set; }

    public Dictionary<string, string> Tags { get; set; } = null!;

    public string Name => Tags.ContainsKey("name") ? Tags["name"] : "Unknown";
}