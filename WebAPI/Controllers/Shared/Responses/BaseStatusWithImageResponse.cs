namespace WebAPI.Controllers.Shared.Responses;

public class BaseStatusWithImageResponse : BaseStatusResponse
{
    public required string? Image { get; set; }
}