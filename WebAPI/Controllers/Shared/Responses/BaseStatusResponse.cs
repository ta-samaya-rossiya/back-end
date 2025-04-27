namespace WebAPI.Controllers.Shared.Responses;

public class BaseStatusResponse
{
    public required bool Completed { get; set; }
    
    public required string Message { get; set; }
}