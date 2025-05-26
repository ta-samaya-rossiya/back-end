using System.Globalization;
using Domain.Entities;
using Domain.Enums;
using NetTopologySuite.Geometries;
using WebAPI.Controllers.HistoricalLines.Responses;
using WebAPI.Controllers.Regions.Responses;

namespace WebAPI.Controllers.Utility;

public static class DtoConvert
{
    public static HistoricalObjectInfoResponse HistoricalObjectToResponse(HistoricalObject historicalObject, HttpContext httpContext)
    {
        return new HistoricalObjectInfoResponse
        {
            Id = historicalObject.Id,
            Title = historicalObject.Title,
            Image = ConvertImagePathToUrl(historicalObject.ImagePath,
                httpContext),
            Description = historicalObject.Description,
            VideoUrl = historicalObject.VideoUrl,
            Order = historicalObject.Order,
            Coords = ConvertPointToLatLon(historicalObject.Coordinates)
        };
    }
    
    public static FullLineInfoResponse GetFullLineResponse(HistoricalLine line, HistoricalObject[] objects,
        Region[] addedRegions, Region[] activeRegions, HttpContext httpContext)
    {
        var result = new FullLineInfoResponse
        {
            Id = line.Id,
            Title = line.Title,
            MarkerImage = ConvertImagePathToUrl(line.MarkerImagePath,
                httpContext)!,
            LineColor = line.LineColor,
            LineStyle = line.LineStyle.ToResponseString(),
            MarkerLegend = line.MarkerLegend,
            IsActive = line.IsActive,
            Markers = objects.OrderBy(o => o.Order)
                .Select((o,
                    i) => new MarkerInfo
                {
                    Id = o.Id,
                    Title = o.Title,
                    Coords = ConvertPointToLatLon(o.Coordinates),
                    Order = o.Order
                })
                .ToArray(),
            AddedRegions = addedRegions.Select(r => new AddedRegionInfo
                {
                    Id = r.Id,
                    Title = r.Title,
                    DisplayTitle = new DisplayTitleResponse
                    {
                        Text = r.DisplayTitle ?? "",
                        Position = ConvertPointToLatLon(r.DisplayTitlePosition),
                        FontSize = r.DisplayTitleFontSize
                    },
                    Color = r.FillColor
                })
                .ToArray(),
            ActiveRegions = activeRegions.Select(r => new AddedRegionInfo
                {
                    Id = r.Id,
                    Title = r.Title,
                    DisplayTitle = new DisplayTitleResponse
                    {
                        Text = r.DisplayTitle ?? "",
                        Position = ConvertPointToLatLon(r.DisplayTitlePosition),
                        FontSize = r.DisplayTitleFontSize
                    },
                    Color = r.FillColor
                })
                .ToArray(),
        };
        return result;
    }
    
    
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
            IsActive = regionInLines.Any(l => l.RegionId == r.Id && l.IsActive),
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

    public static string? ConvertImagePathToUrl(string? imagePath, HttpContext httpContext)
    {
        if (imagePath == null)
        {
            return null;
        }
        var request = httpContext.Request;
        var baseUrl = $"{request.Scheme}://{request.Host}";
        var relativePath = imagePath.Replace("wwwroot", "");
        
        var fullUrl = baseUrl + "/" + relativePath;
        return fullUrl.Replace("\\", "/");
    }
    
    public static double[] ConvertPointToLatLon(Point point)
    {
        var coordinate = point.Coordinate;
        return new double[] { coordinate.Y, coordinate.X };
    }
    
    public static Point ConvertLatLonToPoint(double y, double x)
    {
        return new Point(x, y);
    }
    
    public static double[][][] ConvertPolygonToLatLon(MultiPolygon multiPolygon)
    {
        var result = new List<double[][]>();

        for (var i = 0; i < multiPolygon.NumGeometries; i++)
        {
            var polygon = (Polygon)multiPolygon.GetGeometryN(i);
            var rings = new List<double[][]>();

            // Внешнее кольцо
            var exterior = polygon.ExteriorRing.Coordinates
                .Select(c => new double[] { c.Y, c.X })
                .ToArray();
            rings.Add(exterior);

            // Внутренние кольца
            for (var j = 0; j < polygon.NumInteriorRings; j++)
            {
                var interior = polygon.GetInteriorRingN(j).Coordinates
                    .Select(c => new double[] { c.Y, c.X })
                    .ToArray();
                rings.Add(interior);
            }

            result.AddRange(rings.ToArray());
        }

        return result.ToArray();
    }
}