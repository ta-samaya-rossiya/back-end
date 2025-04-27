using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;

namespace Domain.Entities;

/// <summary>
/// Показатели региона
/// </summary>
public class RegionIndicators : IHasId
{
    [Key]
    public required Guid Id { get; set; }
    
    /// <summary>
    /// Идентификатор региона, к которому относятся показатели
    /// </summary>
    public required Guid RegionId { get; set; }
    [ForeignKey("RegionId")]
    public Region Region { get; set; }
    
    /// <summary>
    /// Путь к изображению для всплывающего окна показателей региона
    /// </summary>
    public string? ImagePath { get; set; }
    
    /// <summary>
    /// Кол-во экскурсий в регионе
    /// </summary>
    public int Excursions { get; set; }
    
    /// <summary>
    /// Кол-во партнеров в регионе
    /// </summary>
    public int Partners { get; set; }
    
    /// <summary>
    /// Кол-во участников в регионе
    /// </summary>
    public int Participants { get; set; }
}