using System.ComponentModel.DataAnnotations;
using Fringe.Domain.DTOs.TicketPriceDTOs;

namespace Fringe.Domain.DTOs.PerformanceDTOs;

public class UpdatePerformanceDto
{
    [Required]
    public DateTime PerformanceDate { get; set; }
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    
    public bool SoldOut { get; set; }
    public bool Cancel { get; set; }
    public bool Active { get; set; }
    
    public List<PerformanceTicketPriceDto> TicketPrices { get; set; } = new List<PerformanceTicketPriceDto>();

}