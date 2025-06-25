

using System.ComponentModel.DataAnnotations;

namespace Fringe.Domain.DTOs.TicketTypeDTOs;

public class UpdateTicketTypeDto
{
    [Required]
    [StringLength(100)]
    public string TypeName { get; set; } = string.Empty;
    
    [StringLength(200)]
    public string? Description { get; set; }
}