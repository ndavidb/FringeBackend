using Fringe.Domain.DTOs.TicketPriceDTOs;

namespace Fringe.Service.Interfaces;

public interface ITicketPriceService
{
    Task<IEnumerable<TicketPriceDto>> GetAllTicketPricesAsync();
    Task<TicketPriceDto?> GetTicketPriceByIdAsync(int ticketPriceId);
    Task<IEnumerable<TicketPriceDto>> GetTicketPricesByPerformanceIdAsync(int performanceId);
    Task<IEnumerable<TicketPriceDto>> GetTicketPricesByTicketTypeIdAsync(int ticketTypeId);
    Task<TicketPriceDto> CreateTicketPriceAsync(CreateTicketPriceDto dto, string creatorUserId);
    Task<TicketPriceDto> UpdateTicketPriceAsync(int ticketPriceId, UpdateTicketPriceDto dto, string updaterUserId);
    Task<bool> DeleteTicketPriceAsync(int ticketPriceId);
}