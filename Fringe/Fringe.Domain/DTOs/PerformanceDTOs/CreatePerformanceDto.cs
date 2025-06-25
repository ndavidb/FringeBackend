using System.ComponentModel.DataAnnotations;
using Fringe.Domain.DTOs.TicketPriceDTOs;
using Fringe.Domain.Enum;

namespace Fringe.Domain.DTOs.PerformanceDTOs;

public class CreatePerformanceDto
{
    [Required]
    public int ShowId { get; set; }
    
    [Required]
    public DateTime PerformanceDate { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    public SeatingType SeatingType { get; set; } = SeatingType.GeneralAdmission;

    
    public bool SoldOut { get; set; } = false;
    public bool Cancel { get; set; } = false;
    public bool Active { get; set; } = true;
    
    public List<PerformanceTicketPriceDto> TicketPrices { get; set; } = new List<PerformanceTicketPriceDto>();

}