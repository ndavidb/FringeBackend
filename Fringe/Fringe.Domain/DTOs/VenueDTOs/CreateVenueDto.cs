﻿namespace Fringe.Domain.DTOs.VenueDTOs;

public class CreateVenueDto
{
    public string VenueName { get; set; }
    public int TypeId { get; set; }
    public int MaxCapacity { get; set; }
    public string Description { get; set; }
    public string ContactEmail { get; set; }
    public string ContactPhone { get; set; }
    public string ImagesUrl { get; set; }
    public bool Active { get; set; }
    public string VenueUrl { get; set; }
    public int LocationId { get; set; }
    
    // Seating Plan properties
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}
