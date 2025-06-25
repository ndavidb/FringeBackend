using Fringe.Domain.DTOs.TicketTypeDTOs;

namespace Fringe.Service.Interfaces;

public interface ITicketTypeService
{
    Task<IEnumerable<TicketTypeDto>> GetAllTicketTypesAsync();
    Task<TicketTypeDto?> GetTicketTypeByIdAsync(int ticketTypeId);
    Task<TicketTypeDto> CreateTicketTypeAsync(CreateTicketTypeDto createTicketTypeDto, string creatorUserId);
    Task<TicketTypeDto> UpdateTicketTypeAsync(int ticketTypeId, UpdateTicketTypeDto updateTicketTypeDto, string updaterUserId);
    Task<bool> DeleteTicketTypeAsync(int ticketTypeId);
}