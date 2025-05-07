namespace WebAPI.Controllers.Regions.Requests;

public class UpdateRegionDisplayTitleRequest
{
    public required string Text { get; set; }
    
    public required double[] Position { get; set; }
    
    public required int FontSize { get; set; }
}