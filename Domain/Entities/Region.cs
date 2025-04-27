using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Domain.Interfaces;
using NetTopologySuite.Geometries;

namespace Domain.Entities;

public class Region : IHasId
{
    [Key]
    public required Guid Id { get; set; }

    public required string Title { get; set; }
    
    [Column(TypeName = "geometry (polygon)")]
    public required Polygon Geometry { get; set; }



}