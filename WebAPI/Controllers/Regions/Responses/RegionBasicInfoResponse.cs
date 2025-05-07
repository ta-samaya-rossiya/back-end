using WebAPI.Controllers.Shared.Responses;

namespace WebAPI.Controllers.Regions.Responses;

public class RegionBasicInfoResponse : BaseStatusResponse
{
    public required Guid Id { get; set; }
    
    public required string Title { get; set; }
}