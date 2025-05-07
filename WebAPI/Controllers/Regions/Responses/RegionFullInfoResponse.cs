namespace WebAPI.Controllers.Regions.Responses;

public class RegionFullInfoResponse
{
    public Guid Id { get; set; }
    
    public required string Title { get; set; }
        
    public required DisplayTitleResponse DisplayTitle { get; set; }
    
    public required string Color { get; set; }
    
    public bool IsActive { get; set; }
    
    public bool ShowIndicators { get; set; }
    
    public required IndicatorsResponse Indicators { get; set; }
    
    public required double[][] Border { get; set; }
}