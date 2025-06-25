using Fringe.Domain;
using Fringe.Domain.DTOs.AuthDTOs;
using Fringe.Domain.DTOs.Booking;
using Fringe.Domain.DTOs.TicketDTO;
using Fringe.Domain.Entities;
using Fringe.Service.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Fringe.Service;

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly IAuthService _authService;
    private readonly IPerformanceRepository _performanceRepository;
    private readonly IReservedSeatRepository _reservedSeatRepository;
    private readonly ILogger<TicketService> _logger;
    private readonly FringeDbContext _context;
    private readonly IQRCodeService _qRCodeService;

    public TicketService(
        ITicketRepository ticketRepository,
        IAuthService authService,
        IPerformanceRepository performanceRepository,
        IReservedSeatRepository reservedSeatRepository,
        ILogger<TicketService> logger,
        IQRCodeService qRCodeService,
        FringeDbContext context)
    {
        _authService = authService;
        _ticketRepository = ticketRepository;
        _performanceRepository = performanceRepository;
        _reservedSeatRepository = reservedSeatRepository;
        _logger = logger;
        _context = context;
        _qRCodeService = qRCodeService;
    }

    public async Task<IEnumerable<TicketDto>> GetAllTicketsAsync()
    {
        _logger.LogInformation("Getting all tickets");
        var tickets = await _ticketRepository.GetAllTicketsAsync();
        return tickets.Select(MapToDto);
    }

    public async Task<TicketDto?> GetTicketByIdAsync(int ticketId)
    {
        _logger.LogInformation($"Getting ticket with ID: {ticketId}");
        var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        return ticket == null ? null : MapToDto(ticket);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByUserIdAsync(Guid userId)
    {
        _logger.LogInformation($"Getting tickets for user ID: {userId}");
        var tickets = await _ticketRepository.GetTicketsByUserIdAsync(userId);
        return tickets.Select(MapToDto);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByBookingReferenceAsync(string bookingReference)
    {
        _logger.LogInformation($"Getting tickets for bookingReference: {bookingReference}");
        var tickets = await _ticketRepository.GetTicketsByBookingReferenceAsync(bookingReference);
        return tickets.Select(MapToDto);
    }
    
    public async Task<IEnumerable<TicketDto>> GetAllTicketsGroupedByBookingReferenceAsync()
    {
        _logger.LogInformation("Getting all tickets grouped by booking reference");
        var tickets = await _ticketRepository.GetGroupedTicketsByBookingReferenceAsync();
        return tickets.Select(MapToDto);
    }

    public async Task<IEnumerable<TicketDto>> GetTicketsByPerformanceIdAsync(int performanceId)
    {
        _logger.LogInformation($"Getting tickets for performance ID: {performanceId}");
        var performance = await _performanceRepository.GetPerformanceByIdAsync(performanceId);
        if (performance == null)
            throw new InvalidOperationException($"Performance with ID {performanceId} not found");
        var tickets = await _ticketRepository.GetTicketsByPerformanceIdAsync(performanceId);
        return tickets.Select(MapToDto);
    }

    public async Task<TicketDto> CreateTicketAsync(CreateTicketDto dto, string creatorUserId, string bookingRef)
    {
        _logger.LogInformation($"Creating new ticket for performance ID: {dto.PerformanceId}");

        // Validate performance exists
        var performance = await _performanceRepository.GetPerformanceByIdAsync(dto.PerformanceId);
        if (performance == null)
            throw new InvalidOperationException($"Performance with ID {dto.PerformanceId} not found");

        // Convert user ID for database storage
        var guidUserId = Guid.Parse(creatorUserId);
        var user = _authService.FindUserByEmailAsync(dto.CustomerInfo.Email);
        if (user.Result == null)
        {
            var userDTO = new RegisterDto
            {
                Email = dto.CustomerInfo.Email,
                FirstName = dto.CustomerInfo.FirstName,
                LastName = dto.CustomerInfo.LastName,
                Password = "Password@#123",
                PhoneNumber = dto.CustomerInfo.Phone
            };

            var registerUser = await _authService.RegisterAsync(userDTO);

            guidUserId = _authService.FindUserByEmailAsync(dto.CustomerInfo.Email).Result.Id;
        }
        else
        {
            guidUserId = user.Result.Id;
        }

        // Generate QR code with ticket information
        var qrContent = System.Text.Json.JsonSerializer.Serialize(new
        {
            performanceId = dto.PerformanceId,
            userId = guidUserId,
            timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        });

        // Generate QR code image as base64
        var qrCodeBase64 = _qRCodeService.GenerateQRCodeBase64(qrContent);

        var ticket = new Ticket
        {
            PerformanceId = dto.PerformanceId,
            UserId = guidUserId,
            QRImageUrl = $"data:image/png;base64,{qrCodeBase64}",
            QRInCode = bookingRef,
            StartTime = performance.StartTime,
            EndTime = performance.EndTime,
            IsCheckedIn = false,
            Cancelled = false,
            CreatedById = 1,
            CreatedAt = DateTime.UtcNow,
            Quantity = dto.Quantity,
            TicketPriceId = dto.TicketPriceId,
            Price = dto.Price,
            ReservedSeats = new List<ReservedSeat>()
        };

        var createdTicket = await _ticketRepository.CreateTicketAsync(ticket);

        // Create reserved seats if provided and seating plan exists
        if (dto.SelectedSeats != null && dto.SelectedSeats.Any())
        {
            var seatingPlanId = performance.Show.Venue.SeatingPlan.SeatingPlanId;

            foreach (var seatDto in dto.SelectedSeats)
            {
                // Check if seat is already reserved
                var existingReservation = await _context.ReservedSeats
                    .AnyAsync(rs => rs.SeatingPlanId == seatingPlanId &&
                                    rs.RowNumber == seatDto.RowNumber &&
                                    rs.SeatNumber == seatDto.SeatNumber &&
                                    _context.Tickets.Any(t => t.TicketId == rs.TicketId &&
                                                              t.PerformanceId == dto.PerformanceId &&
                                                              !t.Cancelled));

                if (existingReservation)
                {
                    throw new InvalidOperationException(
                        $"Seat {seatDto.RowNumber}-{seatDto.SeatNumber} is already reserved");
                }

                var reservedSeat = new ReservedSeat
                {
                    TicketId = createdTicket.TicketId,
                    SeatingPlanId = seatingPlanId,
                    RowNumber = seatDto.RowNumber,
                    SeatNumber = seatDto.SeatNumber
                };

                await _reservedSeatRepository.CreateReservedSeatAsync(reservedSeat);
            }
        }

        // Get the full ticket with navigational properties
        var fullTicket = await _ticketRepository.GetTicketByIdAsync(createdTicket.TicketId);
        return MapToDto(fullTicket!);
    }

    public async Task<TicketDto> UpdateTicketAsync(int ticketId, UpdateTicketDto dto, string updaterUserId)
    {
        _logger.LogInformation($"Updating ticket with ID: {ticketId}");

        var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticket == null)
            throw new InvalidOperationException($"Ticket with ID {ticketId} not found");

        var guidUserId = Guid.Parse(updaterUserId);
        var updaterId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        ticket.IsCheckedIn = dto.IsCheckedIn;
        ticket.Cancelled = dto.Cancelled;
        ticket.UpdatedId = updaterId;
        ticket.UpdatedAt = DateTime.UtcNow;

        var updatedTicket = await _ticketRepository.UpdateTicketAsync(ticket);
        return MapToDto(updatedTicket);
    }
    
    public async Task<IEnumerable<TicketDto>> UpdateTicketsByBookingRefAsync(string bookingRef, UpdateTicketDto dto, string updaterUserId)
    {
        _logger.LogInformation($"Updating tickets with bookingRef: {bookingRef}");

        var tickets = await _ticketRepository.GetTicketsByBookingReferenceAsync(bookingRef);
        if (!tickets.Any())
            throw new InvalidOperationException($"No tickets found for bookingRef {bookingRef}");

        var updaterId = (int)(Guid.Parse(updaterUserId).GetHashCode() & 0x7FFFFFFF);

        foreach (var ticket in tickets)
        {
            ticket.IsCheckedIn = dto.IsCheckedIn;
            ticket.Cancelled = dto.Cancelled;
            ticket.UpdatedId = updaterId;
            ticket.UpdatedAt = DateTime.UtcNow;
        }

        var updatedTickets = await _ticketRepository.UpdateTicketsByBookingRefAsync(tickets.ToList());
        return updatedTickets.Select(MapToDto).ToList();
    }

    public async Task<bool> DeleteTicketsByBookingRefAsync(string bookingRef)
    {
        _logger.LogInformation($"Deleting tickets by BookingRef: {bookingRef}");

        var tickets = await _ticketRepository.GetTicketsByBookingReferenceAsync(bookingRef);
        if (!tickets.Any())
            throw new InvalidOperationException($"No tickets found for bookingRef: {bookingRef}");

        return await _ticketRepository.DeleteTicketsByBookingRefAsync(bookingRef);
    }

    public async Task<bool> DeleteTicketAsync(int ticketId)
    {
        _logger.LogInformation($"Deleting ticket with ID: {ticketId}");

        if (!await _ticketRepository.TicketExistsAsync(ticketId))
            throw new InvalidOperationException($"Ticket with ID {ticketId} not found");

        return await _ticketRepository.DeleteTicketAsync(ticketId);
    }

    public async Task<bool> CheckInTicketAsync(int ticketId, string updaterUserId)
    {
        _logger.LogInformation($"Checking in ticket with ID: {ticketId}");

        var ticket = await _ticketRepository.GetTicketByIdAsync(ticketId);
        if (ticket == null)
            throw new InvalidOperationException($"Ticket with ID {ticketId} not found");

        if (ticket.Cancelled)
            throw new InvalidOperationException($"Cannot check in a cancelled ticket");

        if (ticket.IsCheckedIn)
            return true; // Already checked in

        var guidUserId = Guid.Parse(updaterUserId);
        var updaterId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        ticket.IsCheckedIn = true;
        ticket.UpdatedId = updaterId;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _ticketRepository.UpdateTicketAsync(ticket);
        return true;
    }

    private static TicketDto MapToDto(Ticket ticket)
    {
        return new TicketDto
        {
            TicketId = ticket.TicketId,
            PerformanceId = ticket.PerformanceId,
            ShowName = ticket.Performance?.Show?.ShowName ?? string.Empty,
            VenueName = ticket.Performance?.Show?.Venue?.VenueName ?? string.Empty,
            UserId = ticket.UserId,
            UserEmail = ticket.User?.Email ?? string.Empty,
            UserName = $"{ticket.User?.FirstName ?? string.Empty} {ticket.User?.LastName ?? string.Empty}".Trim(),
            QRImageUrl = ticket.QRImageUrl,
            QRInCode = ticket.QRInCode,
            StartTime = ticket.StartTime,
            EndTime = ticket.EndTime,
            PerformanceDate = ticket.Performance?.PerformanceDate ?? DateTime.MinValue,
            IsCheckedIn = ticket.IsCheckedIn,
            Cancelled = ticket.Cancelled,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            ReservedSeats = ticket.ReservedSeats?.Select(rs => new ReservedSeatDto
            {
                ReservedSeatId = rs.ReservedSeatId,
                RowNumber = rs.RowNumber,
                SeatNumber = rs.SeatNumber
            }).ToList() ?? new List<ReservedSeatDto>(),
            // These would come from related entities in a full implementation
            Price = ticket.Price, // This would come from TicketPrice entity in a real implementation 
            TicketTypeName = string.Empty // This would come from TicketType entity in a real implementation
        };
    }
}