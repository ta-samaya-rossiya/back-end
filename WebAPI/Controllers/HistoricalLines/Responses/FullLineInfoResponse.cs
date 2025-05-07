namespace WebAPI.Controllers.HistoricalLines.Responses;

public class FullLineInfoResponse
{
    public required Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public required string MarkerImage { get; set; }
    
    public required string LineColor { get; set; }
    
    public required string LineStyle { get; set; }
    
    public required string MarkerLegend { get; set; }
    
    public required bool IsActive { get; set; }
    
    public required MarkerInfo[] Markers { get; set; }
    
    public required AddedRegionInfo[] AddedRegions { get; set; }
    
    public required AddedRegionInfo[] ActiveRegions { get; set; }
}