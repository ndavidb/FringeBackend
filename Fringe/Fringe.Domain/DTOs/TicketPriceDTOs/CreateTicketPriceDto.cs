namespace Fringe.Domain.DTOs.TicketPriceDTOs;

public class CreateTicketPriceDto
{
    public int TicketTypeId { get; set; }
    public int PerformanceId { get; set; }
    public decimal Price { get; set; }
}