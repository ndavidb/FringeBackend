using Fringe.Domain.DTOs.TicketDTO;

namespace Fringe.Service.Interfaces;

public interface ITicketService
{
    Task<IEnumerable<TicketDto>> GetAllTicketsAsync();
    Task<TicketDto?> GetTicketByIdAsync(int ticketId);
    Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(Guid userId);
    Task<IEnumerable<TicketDto>> GetTicketsByBookingReferenceAsync(string bookingReference);
    Task<IEnumerable<TicketDto>> GetAllTicketsGroupedByBookingReferenceAsync();
    Task<IEnumerable<TicketDto>> GetTicketsByPerformanceIdAsync(int performanceId);
    Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, string creatorUserId, string bookingRef);
    Task<TicketDto> UpdateTicketAsync(int ticketId, UpdateTicketDto dto, string updaterUserId);

    Task<IEnumerable<TicketDto>> UpdateTicketsByBookingRefAsync(string bookingRef, UpdateTicketDto dto,
        string updaterUserId);

    Task<bool> DeleteTicketsByBookingRefAsync(string bookingRef);
    Task<bool> DeleteTicketAsync(int ticketId);
    Task<bool> CheckInTicketAsync(int ticketId, string updaterUserId);
}