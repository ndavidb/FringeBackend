using System.Security.Claims;
using Fringe.Domain.DTOs.TicketDTO;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fringe.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;
    private readonly ILogger<TicketsController> _logger;

    public TicketsController(ITicketService ticketService, ILogger<TicketsController> logger)
    {
        _ticketService = ticketService;
        _logger = logger;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,StaffMember")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAllTickets()
    {
        try
        {
            var tickets = await _ticketService.GetAllTicketsAsync();
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tickets");
            return StatusCode(500, new { message = "An error occurred while retrieving tickets." });
        }
    }
    
    [HttpGet("group-by-booking")]
    [Authorize(Roles = "Admin,StaffMember")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetAllTicketsGroupByBookingReference()
    {
        try
        {
            var tickets = await _ticketService.GetAllTicketsGroupedByBookingReferenceAsync();
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting all tickets group by booking reference");
            return BadRequest( "An error occurred while retrieving tickets group by booking reference." );
        }
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<ActionResult<TicketDto>> GetTicketById(int id)
    {
        try
        {
            var ticket = await _ticketService.GetTicketByIdAsync(id);
            if (ticket == null)
                return NotFound(new { message = $"Ticket with ID {id} not found" });

            // If not admin/staff, verify the ticket belongs to the user
            if (!User.IsInRole("Admin") && !User.IsInRole("StaffMember"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (userId != ticket.UserId.ToString())
                    return Forbid();
            }

            return Ok(ticket);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting ticket with ID: {id}");
            return StatusCode(500, new { message = "An error occurred while retrieving the ticket." });
        }
    }
    
    
    [HttpGet("by-booking/{bookingRef}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketsByBookingRef(string bookingRef)
    {
        try
        {
            var tickets = await _ticketService.GetTicketsByBookingReferenceAsync(bookingRef);
            if (tickets == null || !tickets.Any())
                return NotFound(new { message = $"No tickets found for booking reference {bookingRef}" });

            // If not admin/staff, verify the ticket belongs to the user
            if (!User.IsInRole("Admin") && !User.IsInRole("StaffMember"))
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (tickets.Any(ticket => userId != ticket.UserId.ToString()))
                    return Forbid();
            }

            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting tickets for booking reference: {bookingRef}");
            return StatusCode(500, new { message = "An error occurred while retrieving the tickets." });
        }
    }
    
    

    [HttpGet("user/{userId}")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketsByUserId(Guid userId)
    {
        try
        {
            // If not admin/staff, verify the userId is the current user's id
            if (!User.IsInRole("Admin") && !User.IsInRole("StaffMember"))
            {
                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (currentUserId != userId.ToString())
                    return Forbid();
            }

            var tickets = await _ticketService.GetTicketsByUserIdAsync(userId);
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting tickets for user ID: {userId}");
            return StatusCode(500, new { message = "An error occurred while retrieving tickets." });
        }
    }

    [HttpGet("performance/{performanceId}")]
    [Authorize(Roles = "Admin,StaffMember")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> GetTicketsByPerformanceId(int performanceId)
    {
        try
        {
            var tickets = await _ticketService.GetTicketsByPerformanceIdAsync(performanceId);
            return Ok(tickets);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error getting tickets for performance ID: {performanceId}");
            return BadRequest(ex.Message);
        }
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<TicketDto>> CreateTicket([FromBody] CreateTicketDto dto)
    {
        try
        {
            var bookingReference = $"BK{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identification not found" });

            // If not admin/staff, ensure they can only create tickets for themselves
            if (!User.IsInRole("Admin") && !User.IsInRole("StaffMember") && dto.UserId.ToString() != userId)
            {
                return Forbid();
            }

            var ticket = await _ticketService.CreateTicketAsync(dto, userId, bookingReference);
            return CreatedAtAction(nameof(GetTicketById), new { id = ticket.TicketId }, ticket);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, "Invalid operation during ticket creation");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating ticket");
            return StatusCode(500, new { message = "An error occurred while creating the ticket." });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,StaffMember")]
    public async Task<ActionResult<TicketDto>> UpdateTicket(int id, [FromBody] UpdateTicketDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identification not found" });

            var ticket = await _ticketService.UpdateTicketAsync(id, dto, userId);
            return Ok(ticket);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Invalid operation during ticket update for ID: {id}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating ticket with ID: {id}");
            return StatusCode(500, new { message = "An error occurred while updating the ticket." });
        }
    }
    //Bulk Update /api/Tickets/booking/{bookingRef}
    [HttpPut("booking/{bookingRef}")]
    [Authorize(Roles = "Admin,StaffMember")]
    public async Task<ActionResult<IEnumerable<TicketDto>>> UpdateTicketsByBookingRef(string bookingRef, [FromBody] UpdateTicketDto dto)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identification not found" });

            var tickets = await _ticketService.UpdateTicketsByBookingRefAsync(bookingRef, dto, userId);
            return Ok(tickets);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Validation failed during bulk update for bookingRef: {bookingRef}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Unhandled error during bulk update for bookingRef: {bookingRef}");
            return StatusCode(500, new { message = "Internal server error during ticket bulk update." });
        }
    }


    [HttpPut("{id}/check-in")]
    [Authorize(Roles = "Admin,StaffMember")]
    public async Task<ActionResult> CheckInTicket(int id)
    {
        try
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { message = "User identification not found" });

            await _ticketService.CheckInTicketAsync(id, userId);
            return Ok(new { message = "Ticket successfully checked in" });
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Invalid operation during ticket check-in for ID: {id}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error checking in ticket with ID: {id}");
            return StatusCode(500, new { message = "An error occurred while checking in the ticket." });
        }
    }

   //Bulk Delete /api/Tickets/booking/{bookingRef}
    [HttpDelete("booking/{bookingRef}")]
    [Authorize(Roles = "Admin,StaffMember")]
    public async Task<ActionResult> DeleteTicketsByBookingRef(string bookingRef)
    {
        try
        {
            var result = await _ticketService.DeleteTicketsByBookingRefAsync(bookingRef);
            if (!result)
                return NotFound(new { message = $"No tickets found for bookingRef: {bookingRef}" });

            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Invalid operation during bulk delete for bookingRef: {bookingRef}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting tickets for bookingRef: {bookingRef}");
            return StatusCode(500, new { message = "An error occurred while deleting the tickets." });
        }
    }

    
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,StaffMember")]
    public async Task<ActionResult> DeleteTicket(int id)
    {
        try
        {
            await _ticketService.DeleteTicketAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning(ex, $"Invalid operation during ticket deletion for ID: {id}");
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting ticket with ID: {id}");
            return StatusCode(500, new { message = "An error occurred while deleting the ticket." });
        }
    }
}
