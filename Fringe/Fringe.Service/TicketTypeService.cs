using Fringe.Domain.DTOs.TicketTypeDTOs;
using Microsoft.Extensions.Logging;

namespace Fringe.Service;

public class TicketTypeService : ITicketTypeService
{
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly ILogger<TicketTypeService> _logger;

    public TicketTypeService(
        ITicketTypeRepository ticketTypeRepository,
        ILogger<TicketTypeService> logger)
    {
        _ticketTypeRepository = ticketTypeRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<TicketTypeDto>> GetAllTicketTypesAsync()
    {
        _logger.LogInformation("Retrieving all ticket types");
        try
        {
            var ticketTypes = await _ticketTypeRepository.GetAllTicketTypesAsync();
            _logger.LogInformation("Successfully retrieved {Count} ticket types", ticketTypes.Count());
            return ticketTypes.Select(MapToDto);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving all ticket types");
            throw;
        }
    }

    public async Task<TicketTypeDto?> GetTicketTypeByIdAsync(int ticketTypeId)
    {
        _logger.LogInformation("Retrieving ticket type with ID: {TicketTypeId}", ticketTypeId);
        try
        {
            var ticketType = await _ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);
            
            if (ticketType == null)
            {
                _logger.LogWarning("Ticket type with ID {TicketTypeId} not found", ticketTypeId);
                return null;
            }
            
            _logger.LogInformation("Successfully retrieved ticket type with ID: {TicketTypeId}", ticketTypeId);
            return MapToDto(ticketType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while retrieving ticket type with ID: {TicketTypeId}", ticketTypeId);
            throw;
        }
    }

    public async Task<TicketTypeDto> CreateTicketTypeAsync(CreateTicketTypeDto createTicketTypeDto, string creatorUserId)
    {
        _logger.LogInformation("Creating new ticket type with name: {TypeName} by user: {UserId}", 
            createTicketTypeDto.TypeName, creatorUserId);
        
        try
        {
            if (await _ticketTypeRepository.TicketTypeNameExistsAsync(createTicketTypeDto.TypeName))
            {
                _logger.LogWarning("Attempted to create duplicate ticket type with name: {TypeName} by user: {UserId}", 
                    createTicketTypeDto.TypeName, creatorUserId);
                throw new InvalidOperationException($"Ticket type with name {createTicketTypeDto.TypeName} already exists");
            }

            var ticketType = new TicketType
            {
                TypeName = createTicketTypeDto.TypeName,
                Description = createTicketTypeDto.Description!
            };

            var createdTicketType = await _ticketTypeRepository.CreateTicketTypeAsync(ticketType);
            _logger.LogInformation("Successfully created ticket type with ID: {TicketTypeId} by user: {UserId}", 
                createdTicketType.TicketTypeId, creatorUserId);
            
            return MapToDto(createdTicketType);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error occurred while creating ticket type with name: {TypeName} by user: {UserId}", 
                createTicketTypeDto.TypeName, creatorUserId);
            throw;
        }
    }

    public async Task<TicketTypeDto> UpdateTicketTypeAsync(int ticketTypeId, UpdateTicketTypeDto updateTicketTypeDto, string updaterUserId)
    {
        _logger.LogInformation("Updating ticket type with ID: {TicketTypeId} by user: {UserId}", 
            ticketTypeId, updaterUserId);
        
        try
        {
            var existingTicketType = await _ticketTypeRepository.GetTicketTypeByIdAsync(ticketTypeId);
            
            if (existingTicketType == null)
            {
                _logger.LogWarning("Ticket type with ID {TicketTypeId} not found during update attempt by user: {UserId}", 
                    ticketTypeId, updaterUserId);
                throw new InvalidOperationException($"Ticket type with ID {ticketTypeId} not found");
            }
                
            // Check if another ticket type already has this name
            var nameExists = await _ticketTypeRepository.TicketTypeNameExistsAsync(updateTicketTypeDto.TypeName);
            if (nameExists && existingTicketType.TypeName.ToLower() != updateTicketTypeDto.TypeName.ToLower())
            {
                _logger.LogWarning("Attempted to update ticket type to an existing name: {TypeName} by user: {UserId}", 
                    updateTicketTypeDto.TypeName, updaterUserId);
                throw new InvalidOperationException($"Ticket type with name {updateTicketTypeDto.TypeName} already exists");
            }

            _logger.LogDebug("Updating ticket type {TicketTypeId} - Old name: {OldName}, New name: {NewName}",
                ticketTypeId, existingTicketType.TypeName, updateTicketTypeDto.TypeName);

            existingTicketType.TypeName = updateTicketTypeDto.TypeName;
            existingTicketType.Description = updateTicketTypeDto.Description!;

            var updatedTicketType = await _ticketTypeRepository.UpdateTicketTypeAsync(existingTicketType);
            _logger.LogInformation("Successfully updated ticket type with ID: {TicketTypeId} by user: {UserId}", 
                ticketTypeId, updaterUserId);
            
            return MapToDto(updatedTicketType);
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error occurred while updating ticket type with ID: {TicketTypeId} by user: {UserId}", 
                ticketTypeId, updaterUserId);
            throw;
        }
    }

    public async Task<bool> DeleteTicketTypeAsync(int ticketTypeId)
    {
        _logger.LogInformation("Deleting ticket type with ID: {TicketTypeId}", ticketTypeId);
        
        try
        {
            if (!await _ticketTypeRepository.TicketTypeExistsAsync(ticketTypeId))
            {
                _logger.LogWarning("Attempted to delete non-existent ticket type with ID: {TicketTypeId}", ticketTypeId);
                throw new InvalidOperationException($"Ticket type with ID {ticketTypeId} not found");
            }

            var result = await _ticketTypeRepository.DeleteTicketTypeAsync(ticketTypeId);

            if (result)
                _logger.LogInformation("Successfully deleted ticket type with ID: {TicketTypeId}", ticketTypeId);
            else
                _logger.LogWarning("Failed to delete ticket type with ID: {TicketTypeId}", ticketTypeId);
                
            return result;
        }
        catch (Exception ex) when (!(ex is InvalidOperationException))
        {
            _logger.LogError(ex, "Error occurred while deleting ticket type with ID: {TicketTypeId}", ticketTypeId);
            throw;
        }
    }

    private static TicketTypeDto MapToDto(TicketType ticketType)
    {
        return new TicketTypeDto
        {
            TicketTypeId = ticketType.TicketTypeId,
            TypeName = ticketType.TypeName,
            Description = ticketType.Description
        };
    }
}
