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
        services.TryAddScoped<BaseService<HistoricalRoute>>();
        services.TryAddScoped<BaseService<RoutePoint>>();
        services.TryAddScoped<BaseService<HighlightedRegionInRoute>>();
        services.TryAddScoped<IHttpService, HttpService>();
        services.TryAddScoped<OsmRegionSearcher>();
        return services;
    }
}