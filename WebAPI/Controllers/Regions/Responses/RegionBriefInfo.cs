namespace WebAPI.Controllers.Regions.Responses;

public class RegionBriefInfo
{
    public required Guid Id { get; set; }
    
    public required int OsmId { get; set; }
    
    public required string Title { get; set; }
}