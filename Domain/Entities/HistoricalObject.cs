using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;
using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class HistoricalObject : IHasId
{
    [Key]
    public required Guid Id { get; set; }
    
    /// <summary>
    /// Координаты точки объекта на карте
    /// </summary>
    public required Point Coordinates { get; set; }
    
    /// <summary>
    /// Порядковый номер объекта внутри исторической линии
    /// </summary>
    public int Order { get; set; }
    
    /// <summary>
    /// Название объекта
    /// </summary>
    public required string Title { get; set; }
    
    /// <summary>
    /// Путь к изображению в локальном хранилище (images/objects/image.png)
    /// </summary>
    public string? ImagePath { get; set; }
    
    /// <summary>
    /// Описание объекта
    /// </summary>
    public required string Description { get; set; }
    
    /// <summary>
    /// Ссылка на видеоэкскурсию для встраивания
    /// </summary>
    public string? VideoUrl { get; set; }
    
    /// <summary>
    /// Идентификатор исторической линии, к которой принадлежит объект
    /// </summary>
    public required Guid LineId { get; set; }
    [ForeignKey("LineId")]
    public HistoricalLine Line { get; set; }
}