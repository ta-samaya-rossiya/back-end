namespace WebAPI.Controllers.HistoricalLines.Responses;

public class MarkerInfo
{
    public required Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public required double[] Coords { get; set; }
    
    public required int Order { get; set; }
}