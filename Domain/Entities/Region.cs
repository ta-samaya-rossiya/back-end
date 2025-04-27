using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;
using NetTopologySuite.Geometries;

namespace Domain.Entities;

/// <summary>
/// Регион на карте, может быть регионом РФ или другой страной
/// </summary>
public class Region : IHasId
{
    [Key]
    public required Guid Id { get; set; }

    /// <summary>
    /// Название региона (для внутреннего отображения)
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Идентификатор региона на карте OpenStreetMap
    /// </summary>
    public int OsmId {get; set;}
    
    /// <summary>
    /// Полигон, обозначающий границы региона
    /// </summary>
    [Column(TypeName = "geometry (polygon)")]
    public required Polygon Border { get; set; }

    /// <summary>
    /// Отображаемое на карте название региона
    /// </summary>
    public string? DisplayTitle { get; set; }
    
    /// <summary>
    /// Размер шрифта отображаемого на карте названия региона
    /// </summary>
    public int DisplayTitleFontSize { get; set; }
    
    /// <summary>
    /// Метоположение отображаемого на карте названия региона
    /// </summary>
    public required Point DisplayTitlePosition { get; set; }

    /// <summary>
    /// Нужно ли отображать на карте название региона из DisplayTitle
    /// </summary>
    public bool ShowDisplayTitle { get; set; }
    
    /// <summary>
    /// Цвет заливки региона
    /// </summary>
    public required string FillColor { get; set; }
    
    /// <summary>
    /// Нужно ли отображать показатели региона по клику на него во вкладке Показатели
    /// </summary>
    public bool ShowIndicators { get; set; }
    
    /// <summary>
    /// Является ли регион регионом РФ
    /// </summary>
    public bool IsRussia { get; set; }
    
    /// <summary>
    /// Идентификатор показателей региона
    /// </summary>
    public Guid IndicatorsId { get; set; }
    [ForeignKey("IndicatorsId")]
    public RegionIndicators Indicators { get; set; }
}