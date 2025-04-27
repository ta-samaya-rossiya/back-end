using System.ComponentModel.DataAnnotations;
using Domain.Interfaces;

namespace Domain.Entities;

public class HistoricalRoute : IHasId
{
    [Key]
    public required Guid Id { get; set; }
    
    public required string Title { get; set; }
    
    public string? Description { get; set; }
    
    public string? ImagePath { get; set; }
    
    
}