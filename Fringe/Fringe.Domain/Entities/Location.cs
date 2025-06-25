using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fringe.Domain.Entities;

// Domain entity representing the Location details
public class Location
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int LocationId { get; set; }
    public string LocationName { get; set; }
    public string Address { get; set; }
    public string Suburb { get; set; }
    public string PostalCode { get; set; }
    public string State { get; set; }
    public string Country { get; set; }
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public bool ParkingAvailable { get; set; }
    public bool Active { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? UpdatedId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public List<Venue> Venues { get; set; }
}