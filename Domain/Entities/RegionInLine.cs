using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Entities;

/// <summary>
/// Связь регионов и исторических линий
/// </summary>
public class RegionInLine : IHasId
{
    [Key]
    public required Guid Id { get; set; }
    
    /// <summary>
    /// Идентификатор исторической линии
    /// </summary>
    public required Guid LineId { get; set; }
    [ForeignKey("LineId")] 
    public HistoricalLine Line { get; set; } = null!;
    
    /// <summary>
    /// Идентификатор региона
    /// </summary>
    public required Guid RegionId { get; set; }
    [ForeignKey("RegionId")]
    public Region Region { get; set; } = null!;
    
    /// <summary>
    /// Состояние региона на исторической линии (подсвечивается или нет)
    /// </summary>
    public bool IsActive { get; set; }
}