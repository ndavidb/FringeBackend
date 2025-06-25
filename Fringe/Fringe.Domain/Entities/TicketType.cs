namespace Fringe.Domain.Entities;

public class TicketType
{
    public int TicketTypeId { get; set; }
    public string TypeName { get; set; } // e.g., "Standard", "VIP", "Group"
    public string Description { get; set; }
    public List<TicketPrice> TicketPrices { get; set; }
}