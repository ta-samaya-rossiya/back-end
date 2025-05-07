namespace WebAPI.Controllers.Regions.Responses;

public class DisplayTitleResponse
{
    public required string Text { get; set; }
    
    public required double[] Position { get; set; }
    
    public int FontSize { get; set; }
}