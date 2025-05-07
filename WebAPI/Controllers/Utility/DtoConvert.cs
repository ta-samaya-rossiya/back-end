using System.Globalization;
using Domain.Entities;
using NetTopologySuite.Geometries;
using WebAPI.Controllers.Regions.Responses;

namespace WebAPI.Controllers.Utility;

public static class DtoConvert
{
    public static RegionsFullInfoResponse RegionsToFullInfoResponse(Region[] regions, RegionIndicators[] regionsIndicators, RegionInLine[] regionInLines, HttpContext httpContext)
    {
        var list = regions.Select(r => new RegionFullInfoResponse
        {
            Id = r.Id,
            Title = r.Title,
            DisplayTitle = new DisplayTitleResponse
            {
                Text = r.DisplayTitle ?? "",
                Position = ConvertPointToLatLon(r.DisplayTitlePosition),
                FontSize = r.DisplayTitleFontSize
            },
            Color = r.FillColor,
            IsActive = regionInLines.Length != 0 && regionInLines.Any(l => l.RegionId == r.Id),
            ShowIndicators = true,
            Indicators = ConvertIndicatorsResponse(regionsIndicators.First(i => i.RegionId == r.Id), httpContext),
            Border = ConvertPolygonToLatLon(r.Border)
        }).ToArray();
        return new RegionsFullInfoResponse
        {
            Regions = list
        };
    }
    
    public static IndicatorsResponse ConvertIndicatorsResponse(RegionIndicators indicators, HttpContext httpContext)
    {
        return new IndicatorsResponse
        {
            CoatOfArms = ConvertImagePathToUrl(indicators.ImagePath!, httpContext),
            Excursions = indicators.Excursions,
            Partners = indicators.Partners,
            Participants = indicators.Participants,
        };
    }

    public static string ConvertImagePathToUrl(string imagePath, HttpContext httpContext)
    {
        var request = httpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var wwwrootPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var relativePath = imagePath.Replace(wwwrootPath, "").Replace("\\", "/");
        
        var fullUrl = baseUrl + relativePath;
        return fullUrl.Replace("\\", "/");
    }
    
    public static double[] ConvertPointToLatLon(Point point)
    {
        var coordinate = point.Coordinate;
        return new double[] { coordinate.Y, coordinate.X };
    }
    
    public static double[][] ConvertPolygonToLatLon(Polygon polygon)
    {
        // Список для хранения всех координат

        // Получаем внешнее кольцо полигона
        var exteriorCoordinates = polygon.ExteriorRing.Coordinates;
        var coordinatesList = exteriorCoordinates.Select(coord => new double[] { coord.Y, coord.X }).ToList();

        // Обрабатываем внутренние кольца (если есть)
        foreach (var interiorRing in polygon.InteriorRings)
        {
            Coordinate[] interiorCoordinates = interiorRing.Coordinates;
            coordinatesList.AddRange(interiorCoordinates.Select(coord => new double[] { coord.Y, coord.X }));
        }

        // Возвращаем массив массивов (double[][])
        return coordinatesList.ToArray();
    }
}