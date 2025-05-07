using WebAPI.Controllers.HistoricalLines.Responses;

namespace WebAPI.AdminControllers.HistoricalObjects.Responses;

public class GetObjectsInLineResponse
{
    public required HistoricalObjectInfoResponse[] Objects { get; set; }
}