using WebAPI.Controllers.Shared.Responses;

namespace WebAPI.Controllers.Regions.Responses;

public class OsmSearchResponse : BaseStatusResponse
{
    public required OsmRegionBasicInfo[] Results { get; set; }
}