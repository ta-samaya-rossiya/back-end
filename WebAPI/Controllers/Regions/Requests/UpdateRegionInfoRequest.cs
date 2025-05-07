namespace WebAPI.Controllers.Regions.Requests;

public class UpdateRegionInfoRequest
{
    public required string Title { get; set; }
    
    public required UpdateRegionDisplayTitleRequest DisplayTitle { get; set; }
    
    public required string Color { get; set; }
    
    public required bool ShowIndicators { get; set; }
    
    public required UpdateRegionIndicatorsRequest Indicators { get; set; }
}