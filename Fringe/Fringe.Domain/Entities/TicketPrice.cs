namespace Fringe.Domain.Entities;

public class TicketPrice
{
    public int TicketPriceId { get; set; }
    public int TicketTypeId { get; set; }
    public int PerformanceId { get; set; }
    public decimal Price { get; set; }
    
    public Performance Performance { get; set; }
    public TicketType TicketType { get; set; }
}