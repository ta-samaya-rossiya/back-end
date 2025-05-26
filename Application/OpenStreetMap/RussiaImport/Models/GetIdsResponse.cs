using Application.OpenStreetMap.SharedModels;

namespace Application.OpenStreetMap.RussiaImport.Models;

public class GetIdsResponse : BaseOverpassApiResponse
{
    public IdElement[] Elements { get; set; }
}