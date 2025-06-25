namespace Fringe.Domain.DTOs.LocationDTOs;

public class CreateLocationDto
{
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
}