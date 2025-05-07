using System.Diagnostics;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using NetTopologySuite.IO;
using NetTopologySuite.Simplify;

namespace Application.OpenStreetMap;

public static class GeometrySimplifier
{
    public static async Task<Geometry> SimplifyToPercentageAsync(string geoJson)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "OsmToGeoJson");
        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = "Simplify.js",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = dir
        };

        var process = new Process { StartInfo = startInfo };
        process.Start();

        // Передаём входной geojson в скрипт
        await process.StandardInput.WriteAsync(geoJson);
        await process.StandardInput.FlushAsync();
        process.StandardInput.Close();

        // Читаем результат
        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Ошибка при упрощении GeoJSON: {error}");
        }
        var reader = new GeoJsonReader();
        var result = reader.Read<FeatureCollection>(output).First().Geometry!;
        
        return result;
    }
}