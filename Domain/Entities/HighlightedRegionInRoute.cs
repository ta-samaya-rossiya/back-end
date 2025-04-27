using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Entities;

public class HighlightedRegionInRoute : IHasId
{
    [Key]
    public required Guid Id { get; set; }

    public required Guid RouteId { get; set; }
    [ForeignKey("RouteId")] 
    public HistoricalRoute Route { get; set; } = null!;
    
    public required Guid RegionId { get; set; }
    [ForeignKey("RegionId")]
    public Region Region { get; set; } = null!;
}