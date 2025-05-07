using Application.OpenStreetMap.RegionSearch;
using Application.Services;
using Application.Services.Http;
using Domain.Entities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Application;

public static class ApplicationStartup
{
    public static IServiceCollection AddApplicationLayer(this IServiceCollection services)
    {
        services.TryAddScoped<BaseService<Region>>();
        services.TryAddScoped<BaseService<HistoricalLine>>();
        services.TryAddScoped<BaseService<HistoricalObject>>();
        services.TryAddScoped<BaseService<RegionInLine>>();
        services.TryAddScoped<BaseService<RegionIndicators>>();
        
        services.TryAddScoped<IHttpService, HttpService>();
        services.TryAddScoped<OsmRegionSearcher>();
        services.TryAddScoped<OsmNewRegionsService>();
        services.TryAddScoped<RegionImageService>();
        return services;
    }
}