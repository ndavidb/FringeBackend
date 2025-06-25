using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fringe.Domain.Entities;

public class Ticket
{
    public int TicketId { get; set; }
    public int PerformanceId { get; set; }
    public Guid UserId { get; set; }
    public string QRImageUrl { get; set; }
    public string QRInCode { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool IsCheckedIn { get; set; }
    public bool Cancelled { get; set; }
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    public int? TicketPriceId { get; set; }
    public int? Quantity { get; set; }
    public decimal Price { get; set; }

    // Navigational properties
    public Performance Performance { get; set; }
    public ApplicationUser User { get; set; }
    public List<ReservedSeat>? ReservedSeats { get; set; }
}