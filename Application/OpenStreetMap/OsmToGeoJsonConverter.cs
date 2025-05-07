using System.Diagnostics;
using GeoJSON.Net.Feature;
using GeoJSON.Net.Geometry;
using OsmSharp;
using OsmSharp.Streams;
using Serilog;

namespace Application.OpenStreetMap;

public static class OsmToGeoJsonConverter
{
    public static async Task<string> OsmToGeoJsonAsync(string osmData)
    {
        var dir = Path.Combine(Directory.GetCurrentDirectory(), "OsmToGeoJson");
        var startInfo = new ProcessStartInfo
        {
            FileName = "node",
            Arguments = "OsmToGeoJsonConvert.js",
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = dir
        };

        using var process = new Process();
        process.StartInfo = startInfo;
        process.Start();

        await process.StandardInput.WriteAsync(osmData);
        process.StandardInput.Close();

        var output = await process.StandardOutput.ReadToEndAsync();
        var error = await process.StandardError.ReadToEndAsync();

        await process.WaitForExitAsync();

        if (process.ExitCode != 0)
        {
            throw new Exception($"Node.js процесс завершился с ошибкой: {error}");
        }

        return output;
    }
}