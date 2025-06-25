using System.ComponentModel.DataAnnotations;

namespace Fringe.Domain.DTOs.TicketTypeDTOs;

public class CreateTicketTypeDto
{
    [Required]
    [StringLength(100)]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Property must contain only letters and spaces, no numbers.")]

    public string TypeName { get; set; } = string.Empty;
    
    [StringLength(200)]
    [RegularExpression(@"^[a-zA-Z\s]+$", ErrorMessage = "Property must contain only letters and spaces, no numbers.")]

    public string? Description { get; set; }
}