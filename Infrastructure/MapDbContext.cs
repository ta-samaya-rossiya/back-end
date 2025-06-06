﻿using Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

namespace Infrastructure;

public class MapDbContext : DbContext
{
    public DbSet<Region> Regions { get; set; }
    public DbSet<HistoricalObject> RouteObjects { get; set; }
    public DbSet<HistoricalLine> HistoricalLines { get; set; }
    public DbSet<RegionInLine> RegionInLines { get; set; }
    public DbSet<RegionIndicators> RegionIndicators { get; set; }
    
    private readonly string _connectionString;
    
    public MapDbContext(IConfiguration configuration)
    {
        var readedConnString = configuration.GetConnectionString("MapDBConnection");
        if (readedConnString is null)
        {
            throw new Exception("Connection string \"MapDBConnection\" wasn't found in configuration/appsettings.json");
        }

        _connectionString = readedConnString;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSnakeCaseNamingConvention();
        optionsBuilder.UseNpgsql(_connectionString, x => x.UseNetTopologySuite());
    }
}