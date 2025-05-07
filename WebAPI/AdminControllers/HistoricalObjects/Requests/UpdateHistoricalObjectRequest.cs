namespace WebAPI.AdminControllers.HistoricalObjects.Requests;

public class UpdateHistoricalObjectRequest
{
    public required string Title { get; set; }
    
    public required double[] Coords { get; set; }
    
    public required int Order { get; set; }
    
    public required string Description { get; set; }
    
    public required string? VideoUrl { get; set; }
}