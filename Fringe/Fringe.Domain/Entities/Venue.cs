using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fringe.Domain.Entities;

// Domain entity representing the Venue details
public class Venue
{
    
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int VenueId { get; set; }
    public string VenueName { get; set; }
    public int LocationId { get; set; }
    public int TypeId { get; set; } // Foreign Key to VenueTypeLookUp
    public int MaxCapacity { get; set; }
    public string Description { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public bool IsAccessible { get; set; }
    public string VenueUrl { get; set; }
    public string ImagesUrl { get; set; }
    public bool Active { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    

    public Location Location { get; set; } 
    public VenueTypeLookup VenueTypeLookUp { get; set; }
    public SeatingPlan SeatingPlan { get; set; } 
}