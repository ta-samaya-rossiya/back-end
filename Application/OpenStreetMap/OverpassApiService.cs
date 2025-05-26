namespace Application.OpenStreetMap;

public class OverpassApiService
{
    public async Task<string> GetOverpassApiResponse(string commands)
    {
        using var client = new HttpClient();
        var response = await client.GetAsync($"https://overpass-api.de/api/interpreter?data={commands}");
        response.EnsureSuccessStatusCode();
        var result = await response.Content.ReadAsStringAsync();
        return result;
    }
}