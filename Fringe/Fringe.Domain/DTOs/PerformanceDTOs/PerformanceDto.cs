using Fringe.Domain.DTOs.TicketPriceDTOs;
using Fringe.Domain.DTOs.VenueDTOs;
using Fringe.Domain.Entities;
using Fringe.Domain.Enum;

namespace Fringe.Domain.DTOs.PerformanceDTOs;

public class PerformanceDto
{
    public int PerformanceId { get; set; }
    public int ShowId { get; set; }
    public string ShowName { get; set; } = string.Empty;
    public DateTime PerformanceDate { get; set; }
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public bool SoldOut { get; set; }
    public bool Cancel { get; set; }
    public bool Active { get; set; }
    public SeatingType SeatingType { get; set; }
    public SeatingPlanDto SeatingPlan { get; set; }
    public int RemainingSeats { get; set; }
    public List<ReservedSeatDto> ReservedSeats { get; set; } = new List<ReservedSeatDto>();
    public List<TicketPriceDto> TicketPrices { get; set; } = new List<TicketPriceDto>();
    
}