using System.Security.Claims;
using Fringe.Domain.DTOs.TicketPriceDTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fringe.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketPricesController : Controller
{
    private readonly ITicketPriceService _ticketPriceService;
    private readonly ILogger<TicketPricesController> _logger;

    public TicketPricesController(
        ITicketPriceService ticketPriceService,
        ILogger<TicketPricesController> logger)
    {
        _ticketPriceService = ticketPriceService;
        _logger = logger;
    }


    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllTicketPrices()
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        _logger.LogInformation("User {UserId} - Getting all ticket prices", creatorUserId);
        var ticketPrices = await _ticketPriceService.GetAllTicketPricesAsync();
        return Ok(ticketPrices);
    }


    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTicketPriceById(int id)
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        _logger.LogInformation("User {UserId} - Getting ticket price by id {id}", creatorUserId, id);
        var ticketPrice = await _ticketPriceService.GetTicketPriceByIdAsync(id);
        if (id < 1)
        {
            return BadRequest("Ticket price ID must be greater than 0");
        }
        if (ticketPrice == null)
            return NotFound();
        return Ok(ticketPrice);
    }


    [HttpGet("performance/{performanceId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicketPricesByPerformanceId(int performanceId)
    {
        if (performanceId < 1)
        {
            return BadRequest("PerformanceId  must be greater than 0");
        }
        
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        _logger.LogInformation("User {UserId} - Getting ticket prices for performance ID: {performanceId}",
            creatorUserId, performanceId);
        var ticketPrices = await _ticketPriceService.GetTicketPricesByPerformanceIdAsync(performanceId);
        
        
        
        if (!ticketPrices.Any())
            return NotFound();
        
        return Ok(ticketPrices);
    }


    [HttpGet("tickettype/{ticketTypeId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTicketPricesByTicketTypeId(int ticketTypeId)
    {
        if (ticketTypeId < 1)
        {
            return BadRequest("TicketTypeId  must be greater than 0");
        }   
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        _logger.LogInformation("User {UserId} - Getting ticket prices for ticket type ID: {ticketTypeId}",
            creatorUserId, ticketTypeId);
        var ticketPrices = await _ticketPriceService.GetTicketPricesByTicketTypeIdAsync(ticketTypeId);
        
        if (!ticketPrices.Any())
            return NotFound();
        
        return Ok(ticketPrices);
    }


    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateTicketPrice([FromBody] CreateTicketPriceDto dto)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Creating ticket price", creatorUserId);

            if (string.IsNullOrEmpty(creatorUserId))
            {
                return BadRequest("User ID claim is missing");
            }

            var createdTicketPrice = await _ticketPriceService.CreateTicketPriceAsync(dto, creatorUserId);
            return CreatedAtAction(nameof(GetTicketPriceById), new { id = createdTicketPrice.TicketPriceId },
                createdTicketPrice);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket price");
            return BadRequest(ex.Message);
        }
    }
    
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateTicketPrice(int id, [FromBody] UpdateTicketPriceDto dto)
    {
        try
        {
            var updaterUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Updating ticket price with id {id}", updaterUserId, id);
            var updatedTicketPrice = await _ticketPriceService.UpdateTicketPriceAsync(id, dto, updaterUserId);
            return Ok(updatedTicketPrice);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating ticket price");
            return BadRequest(ex.Message);
        }
    }
    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteTicketPrice(int id)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Deleting ticket price by id {id}", creatorUserId, id);
            await _ticketPriceService.DeleteTicketPriceAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error deleting ticket price");
            return NotFound(ex.Message);
        }
    }
}