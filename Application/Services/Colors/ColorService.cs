using System.Drawing;

namespace Application.Services.Colors;

public static class ColorService
{
    public static List<string> RegionsColors = [
        "#8E2D38", "#4A2336", "#3B293F", "#25253D", "#224058", "#2A6553", 
        "#233F26", "#5F6738", "#D5AC6A", "#786554", "#C17960", "#713535"
    ];
    
    public static string GetRandomColorForRegion()
    {
        return RegionsColors[Random.Shared.Next(0, RegionsColors.Count)];
    }
}