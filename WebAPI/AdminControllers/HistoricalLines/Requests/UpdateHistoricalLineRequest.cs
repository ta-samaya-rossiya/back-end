namespace WebAPI.AdminControllers.HistoricalLines.Requests;

public class UpdateHistoricalLineRequest
{
    public required string Title { get; set; }
    
    public required string LineColor { get; set; }
    
    public required string LineStyle { get; set; }
    
    public required string MarkerLegend { get; set; }
    
    public required bool IsActive { get; set; }
    
    public required string[] ActiveRegionIds { get; set; }
}