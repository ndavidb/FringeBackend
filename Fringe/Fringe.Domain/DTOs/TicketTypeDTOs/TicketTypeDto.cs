namespace Fringe.Domain.DTOs.TicketTypeDTOs;

public class TicketTypeDto
{
    public int TicketTypeId { get; set; }
    public string TypeName { get; set; } = string.Empty;
    public string? Description { get; set; }
}