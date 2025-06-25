namespace Fringe.Domain.Entities;

public class SeatingPlan
{
    public int SeatingPlanId { get; set; }
    public int VenueId { get; set; }
    public int Rows { get; set; }
    public int SeatsPerRow { get; set; }
    public DateTime CreatedAt { get; set; }
    public int CreatedById { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Venue? Venue { get; set; }
    public List<ReservedSeat>? ReservedSeat { get; set; }
}