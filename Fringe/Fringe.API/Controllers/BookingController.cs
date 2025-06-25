using Azure;
using Fringe.Domain.DTOs.Booking;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Fringe.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController> _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController> logger)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        /// <summary>
        /// Create a new booking (creates tickets and reserved seats)
        /// </summary>
        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> CreateBooking([FromBody] CreateBookingDto createBookingDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }
                var result = await _bookingService.CreateBookingAsync(createBookingDto);
                if (result.Success)
                {
                    return Ok(new BookingResponseDto
                    {
                        TicketIds = result.TicketIds,
                        BookingReference = result.BookingReference,
                        Status = result.Status,
                        TotalAmount = result.TotalAmount,
                        Message = "Booking created successfully"
                    });
                }

                return BadRequest(new { error = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return StatusCode(500, new { error = "An error occurred while processing your booking" });
            }
        }

        [HttpGet("confirmation/{bookingReference}")]
        public async Task<IActionResult> GetBookingConfirmation(string bookingReference)
        {
            try
            {
                var confirmation = await _bookingService.GetBookingConfirmationAsync(bookingReference);

                if (confirmation == null)
                {
                    return NotFound(new { error = "Booking not found" });
                }

                return Ok(confirmation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving booking confirmation {BookingReference}", bookingReference);
                return StatusCode(500, new { error = "An error occurred while retrieving the booking confirmation" });
            }
        }

        /// <summary>
        /// Get booking by ticket ID
        /// </summary>
        [HttpGet("ticket/{ticketId}")]
        public async Task<IActionResult> GetBookingByTicketId(int ticketId)
        {
            try
            {
                var booking = await _bookingService.GetBookingByTicketIdAsync(ticketId);

                if (booking == null)
                {
                    return NotFound(new { error = "Ticket not found" });
                }

                return Ok(booking);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving ticket {TicketId}", ticketId);
                return StatusCode(500, new { error = "An error occurred while retrieving the ticket" });
            }
        }

        /// <summary>
        /// Get bookings by user ID
        /// </summary>
        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookingsByUserId(int userId)
        {
            try
            {
                var bookings = await _bookingService.GetBookingsByUserIdAsync(userId);
                return Ok(bookings);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving bookings for user {UserId}", userId);
                return StatusCode(500, new { error = "An error occurred while retrieving bookings" });
            }
        }

        /// <summary>
        /// Cancel a ticket
        /// </summary>
        [HttpPut("ticket/{ticketId}/cancel")]
        public async Task<IActionResult> CancelTicket(int ticketId)
        {
            try
            {
                var result = await _bookingService.CancelTicketAsync(ticketId);

                if (result.Success)
                {
                    return Ok(new { message = "Ticket cancelled successfully" });
                }

                return BadRequest(new { error = result.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling ticket {TicketId}", ticketId);
                return StatusCode(500, new { error = "An error occurred while cancelling the ticket" });
            }
        }
    }
}
