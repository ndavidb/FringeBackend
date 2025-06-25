namespace Fringe.Domain.DTOs.TicketPriceDTOs;

public class TicketPriceDto
{
    public int TicketPriceId { get; set; }
    public string TicketTypeName { get; set; } = string.Empty;
    public decimal Price { get; set; }
}