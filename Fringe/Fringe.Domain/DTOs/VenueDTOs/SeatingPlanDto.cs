namespace Fringe.Domain.DTOs.VenueDTOs;

public class SeatingPlanDto
{
    public int SeatingPlanId { get; set; }
    public int VenueId { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
}
