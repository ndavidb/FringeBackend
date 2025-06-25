using System.ComponentModel.DataAnnotations;
using Fringe.Domain.Enum;

namespace Fringe.Domain.DTOs.PerformanceDTOs;

public class BatchCreatePerformanceDto
{
    [Required]
    public int ShowId { get; set; }
    
    [Required]
    [MinLength(1, ErrorMessage = "At least one performance date is required")]
    public List<DateTime> PerformanceDates { get; set; } = new();
    
    [Required]
    public TimeSpan StartTime { get; set; }
    
    [Required]
    public TimeSpan EndTime { get; set; }
    public SeatingType SeatingType { get; set; } = SeatingType.GeneralAdmission; 

    
    public bool Active { get; set; } = true;
}