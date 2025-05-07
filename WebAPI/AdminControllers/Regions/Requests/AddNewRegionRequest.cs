namespace WebAPI.AdminControllers.Regions.Requests;

public class AddNewRegionRequest
{
    public int RegionId { get; set; }
    
    public Guid? LineId { get; set; }
}