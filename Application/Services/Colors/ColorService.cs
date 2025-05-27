using System.Drawing;

namespace Application.Services.Colors;

public static class ColorService
{
    public static List<string> RegionsColors = [
        "#da4052", "#bf4d84", "#a15db0", "#5a5ab0", "#4992cc", "#48cfa6", 
        "#57c263", "#c7de59", "#f5b651", "#e0aa79", "#f08560", "#db6b6b"
    ];
    
    public static string GetRandomColorForRegion()
    {
        return RegionsColors[Random.Shared.Next(0, RegionsColors.Count)];
    }
    
    public static string GetRandomColorForLine()
    {
        return RegionsColors[Random.Shared.Next(0, RegionsColors.Count)];
    }
}