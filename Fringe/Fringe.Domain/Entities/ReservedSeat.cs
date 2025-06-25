namespace Fringe.Domain.Entities;

public class ReservedSeat
{
    public int ReservedSeatId { get; set; }
    public int SeatingPlanId { get; set; }
    public int? TicketId { get; set; }    
    public int RowNumber { get; set; }
    public int SeatNumber { get; set; }
    
    public SeatingPlan SeatingPlan { get; set; }
    public Ticket Ticket { get; set; }
}