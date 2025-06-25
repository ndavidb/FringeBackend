using Fringe.Domain.DTOs.TicketPriceDTOs;
using Microsoft.Extensions.Logging;

namespace Fringe.Service;

public class TicketPriceService : ITicketPriceService
{
    private readonly ITicketPriceRepository _ticketPriceRepository;
    private readonly ILogger<TicketPriceService> _logger;

    public TicketPriceService(
        ITicketPriceRepository ticketPriceRepository,
        ILogger<TicketPriceService> logger)
    {
        _ticketPriceRepository = ticketPriceRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TicketPriceDto>> GetAllTicketPricesAsync()
    {
        _logger.LogInformation("Getting all ticket prices");
        var ticketPrices = await _ticketPriceRepository.GetAllTicketPricesAsync();
        return ticketPrices.Select(MapToDto);
    }

    public async Task<TicketPriceDto?> GetTicketPriceByIdAsync(int ticketPriceId)
    {
        _logger.LogInformation($"Getting ticket price with ID: {ticketPriceId}");
        var ticketPrice = await _ticketPriceRepository.GetTicketPriceByIdAsync(ticketPriceId);
        return ticketPrice == null ? null : MapToDto(ticketPrice);
    }

    public async Task<IEnumerable<TicketPriceDto>> GetTicketPricesByPerformanceIdAsync(int performanceId)
    {
        _logger.LogInformation($"Getting ticket prices for performance ID: {performanceId}");
        var ticketPrices = await _ticketPriceRepository.GetTicketPricesByPerformanceIdAsync(performanceId);
        return ticketPrices.Select(MapToDto);
    }

    public async Task<IEnumerable<TicketPriceDto>> GetTicketPricesByTicketTypeIdAsync(int ticketTypeId)
    {
        _logger.LogInformation($"Getting ticket prices for ticket type ID: {ticketTypeId}");
        var ticketPrices = await _ticketPriceRepository.GetTicketPricesByTicketTypeIdAsync(ticketTypeId);
        return ticketPrices.Select(MapToDto);
    }

    public async Task<TicketPriceDto> CreateTicketPriceAsync(CreateTicketPriceDto dto, string creatorUserId)
    {
        _logger.LogInformation($"Creating new ticket price by user: {creatorUserId}");

        if (dto.Price < 1)
        {
            throw new InvalidOperationException("Ticket price must be greater than 0");
        }
        
        var ticketPrice = new TicketPrice
        {
            TicketTypeId = dto.TicketTypeId,
            PerformanceId = dto.PerformanceId,
            Price = dto.Price
        };

        var createdTicketPrice = await _ticketPriceRepository.CreateTicketPriceAsync(ticketPrice);
        var result = await _ticketPriceRepository.GetTicketPriceByIdAsync(createdTicketPrice.TicketPriceId);
        
        return MapToDto(result!);
    }

    public async Task<TicketPriceDto> UpdateTicketPriceAsync(int ticketPriceId, UpdateTicketPriceDto dto, string updaterUserId)
    {
        _logger.LogInformation($"Updating ticket price with ID: {ticketPriceId} by user: {updaterUserId}");
        
        var existingTicketPrice = await _ticketPriceRepository.GetTicketPriceByIdAsync(ticketPriceId);
        if (existingTicketPrice == null)
        {
            throw new InvalidOperationException($"Ticket price with ID {ticketPriceId} not found");
        }

        existingTicketPrice.Price = dto.Price;
        
        var updatedTicketPrice = await _ticketPriceRepository.UpdateTicketPriceAsync(existingTicketPrice);
        return MapToDto(updatedTicketPrice);
    }

    public async Task<bool> DeleteTicketPriceAsync(int ticketPriceId)
    {
        _logger.LogInformation($"Deleting ticket price with ID: {ticketPriceId}");
        
        if (!await _ticketPriceRepository.TicketPriceExistsAsync(ticketPriceId))
        {
            throw new InvalidOperationException($"Ticket price with ID {ticketPriceId} not found");
        }
        
        return await _ticketPriceRepository.DeleteTicketPriceAsync(ticketPriceId);
    }

    private static TicketPriceDto MapToDto(TicketPrice ticketPrice)
    {
        return new TicketPriceDto
        {
            TicketPriceId = ticketPrice.TicketPriceId,
            TicketTypeName = ticketPrice.TicketType?.TypeName ?? string.Empty,
            Price = ticketPrice.Price
        };
    }
}