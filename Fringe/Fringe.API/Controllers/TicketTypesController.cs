using System.Security.Claims;
using Fringe.Domain.DTOs.TicketTypeDTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fringe.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketTypesController : ControllerBase
{
    private readonly ITicketTypeService _ticketTypeService;
    private readonly ILogger<TicketTypesController> _logger;

    public TicketTypesController(ITicketTypeService ticketTypeService, ILogger<TicketTypesController> logger)
    {
        _ticketTypeService = ticketTypeService;
        _logger = logger;
    }

    // GET: api/tickettypes
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTicketTypes()
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("User {UserId} - Getting all ticket types", creatorUserId);
        return Ok(await _ticketTypeService.GetAllTicketTypesAsync());
    }

    // GET: api/tickettypes/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketTypeById(int id)
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("User {UserId} - Getting ticket type by id {id}", creatorUserId, id);
        var ticketType = await _ticketTypeService.GetTicketTypeByIdAsync(id);
        if (ticketType == null)
            return NotFound();
        return Ok(ticketType);
    }

    // POST: api/tickettypes
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTicketType([FromBody] CreateTicketTypeDto createTicketTypeDto)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Creating ticket type", creatorUserId);
            
            if (string.IsNullOrEmpty(creatorUserId))
            {
                return BadRequest("User ID claim is missing");
            }
            
            var createdTicketType = await _ticketTypeService.CreateTicketTypeAsync(createTicketTypeDto, creatorUserId);
            return CreatedAtAction(nameof(GetTicketTypeById), new { id = createdTicketType.TicketTypeId }, createdTicketType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket type");
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/tickettypes/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTicketType(int id, [FromBody] UpdateTicketTypeDto updateTicketTypeDto)
    {
        try
        {
            var updaterUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Updating ticket type with id {id}", updaterUserId, id);
            var updatedTicketType = await _ticketTypeService.UpdateTicketTypeAsync(id, updateTicketTypeDto, updaterUserId);
            return Ok(updatedTicketType);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket type");
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/tickettypes/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTicketType(int id)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Deleting ticket type by id {id}", creatorUserId, id);
            await _ticketTypeService.DeleteTicketTypeAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error deleting ticket type");
            return NotFound(ex.Message);
        }
    }
}