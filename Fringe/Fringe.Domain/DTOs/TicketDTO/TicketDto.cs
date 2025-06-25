using Fringe.Domain.DTOs.Booking;

namespace Fringe.Domain.DTOs.TicketDTO;

public class TicketDto
{
    public int TicketId { get; set; }
    public int PerformanceId { get; set; }
    public string ShowName { get; set; } = string.Empty;
    public string VenueName { get; set; } = string.Empty;
    public Guid UserId { get; set; }
    public string UserEmail { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public string QRImageUrl { get; set; } = string.Empty;
    public string QRInCode { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public DateTime PerformanceDate { get; set; }
    public bool IsCheckedIn { get; set; }
    public bool Cancelled { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<ReservedSeatDto> ReservedSeats { get; set; } = new List<ReservedSeatDto>();
    public decimal Price { get; set; }
    public string TicketTypeName { get; set; } = string.Empty;
   
}