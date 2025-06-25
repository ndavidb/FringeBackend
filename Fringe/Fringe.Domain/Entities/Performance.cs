using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Fringe.Domain.Enum;

namespace Fringe.Domain.Entities;

public class Performance
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int PerformanceId { get; set; }
    public int ShowId { get; set; }
    public DateTime PerformanceDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public SeatingType SeatingType { get; set; }
    [NotMapped]
    public int RemainingSeats { get; set; }
    public bool SoldOut { get; set; }
    public bool Cancel { get; set; }
    public bool Active { get; set; } = true;
    
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedId { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public Show Show { get; set; }
    public List<Ticket> Tickets { get; set; } = new List<Ticket>();
    public List<TicketPrice>? TicketPrices { get; set; }
}