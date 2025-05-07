namespace WebAPI.Controllers.Regions.Requests;

public class UpdateRegionIndicatorsRequest
{
    public required int Excursions { get; set; }
    
    public required int Partners { get; set; }
    
    public required int Participants { get; set; }
}