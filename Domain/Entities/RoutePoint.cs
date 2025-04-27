using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;
using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class RoutePoint : IHasId
{
    [Key]
    public required Guid Id { get; set; }
    
    public required Point Coordinates { get; set; }  // Точка (широта, долгота)
    
    public required string Title { get; set; }
    
    public string? AreaDescription { get; set; }
    
    public string? HistoricalDescription { get; set; }
    
    public int Order { get; set; }
    
    public required Guid RouteId { get; set; }
    
    [ForeignKey("RouteId")]
    public HistoricalRoute Route { get; set; }
}