using Fringe.Domain.DTOs.Booking;

namespace Fringe.Domain.DTOs.TicketDTO;

public class CreateTicketDto
{
    public int PerformanceId { get; set; }
    public Guid UserId { get; set; }
    public CustomerInfoDto CustomerInfo { get; set; }
    public int? TicketTypeId { get; set; }
    public int? TicketPriceId { get; set; }
    public int? Quantity { get; set; }
    public decimal Price { get; set; }
    public List<SeatSelectionDto>? SelectedSeats { get; set; }

}