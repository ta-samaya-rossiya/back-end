using WebAPI.Controllers.Regions.Responses;

namespace WebAPI.Controllers.HistoricalLines.Responses;

public class AddedRegionInfo
{
    public required Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public required DisplayTitleResponse DisplayTitle { get; set; }
    
    public required string Color { get; set; }
}