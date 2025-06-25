using System.ComponentModel.DataAnnotations;

namespace Fringe.Domain.DTOs.TicketPriceDTOs;

public class PerformanceTicketPriceDto
{
    [Required]
    public int TicketTypeId { get; set; }
    
    [Required]
    [Range(0, 10000)]
    public decimal Price { get; set; }
}