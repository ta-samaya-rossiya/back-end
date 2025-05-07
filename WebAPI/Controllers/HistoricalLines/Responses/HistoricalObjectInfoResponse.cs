namespace WebAPI.Controllers.HistoricalLines.Responses;

public class HistoricalObjectInfoResponse
{
    public required Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public required string? Image { get; set; }
    
    public required string Description { get; set; }
    
    public required string? VideoUrl { get; set; }
}