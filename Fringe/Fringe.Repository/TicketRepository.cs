using Microsoft.Extensions.Logging;

namespace Fringe.Repository;

public class TicketRepository : ITicketRepository
{
    private readonly FringeDbContext _context;
    private readonly ILogger<TicketRepository> _logger;

    public TicketRepository(FringeDbContext context, ILogger<TicketRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Ticket>> GetAllTicketsAsync()
    {
        _logger.LogInformation("Getting all tickets");
        return await _context.Tickets
            .Include(t => t.Performance)
                .ThenInclude(p => p.Show)
                    .ThenInclude(s => s.Venue)
            .Include(t => t.User)
            .Include(t => t.ReservedSeats)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Ticket?> GetTicketByIdAsync(int ticketId)
    {
        _logger.LogInformation($"Getting ticket with ID: {ticketId}");
        return await _context.Tickets
            .Include(t => t.Performance)
                .ThenInclude(p => p.Show)
                    .ThenInclude(s => s.Venue)
            .Include(t => t.User)
            .Include(t => t.ReservedSeats)
            .FirstOrDefaultAsync(t => t.TicketId == ticketId);
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(Guid userId)
    {
        _logger.LogInformation($"Getting tickets for user ID: {userId}");
        return await _context.Tickets
            .Include(t => t.Performance)
                .ThenInclude(p => p.Show)
                    .ThenInclude(s => s.Venue)
            .Include(t => t.ReservedSeats)
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.Performance.PerformanceDate)
            .ThenByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByBookingReferenceAsync(string bookingReference)
    {
        _logger.LogInformation($"Getting tickets for bookingReference: {bookingReference}");
        return await _context.Tickets
            .Include(t => t.Performance.TicketPrices)
            .Include(t => t.Performance)
                  .ThenInclude(p => p.Show)
                    .ThenInclude(s => s.Venue)
            .Include(t => t.ReservedSeats)
            .Include(t => t.User)
            .Where(t => t.QRInCode == bookingReference) //bookingReference
            .OrderByDescending(t => t.Performance.PerformanceDate)
            .ThenByDescending(t => t.CreatedAt)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetGroupedTicketsByBookingReferenceAsync()
    {
        _logger.LogInformation("Getting grouped tickets by booking reference");
        return await _context.Tickets
            .Include(t => t.Performance)
            .ThenInclude(p => p.Show)
            .ThenInclude(s => s.Venue)
            .Include(t => t.User)
            .Include(t => t.ReservedSeats)
            .GroupBy(t => t.QRInCode)
            .Select(g => g.OrderByDescending(t => t.CreatedAt).First())
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetTicketsByPerformanceIdAsync(int performanceId)
    {
        _logger.LogInformation($"Getting tickets for performance ID: {performanceId}");
        return await _context.Tickets
            .Include(t => t.User)
            .Include(t => t.ReservedSeats)
            .Where(t => t.PerformanceId == performanceId && !t.Cancelled)
            .AsNoTracking()
            .ToListAsync();
    }

    public async Task<Ticket> CreateTicketAsync(Ticket ticket)
    {
        _logger.LogInformation($"Creating new ticket for performance ID: {ticket.PerformanceId}");
        await _context.Tickets.AddAsync(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }

    public async Task<Ticket> UpdateTicketAsync(Ticket ticket)
    {
        _logger.LogInformation($"Updating ticket with ID: {ticket.TicketId}");
        var performance = await _context.Performances.FindAsync(ticket.PerformanceId);
        if (performance != null && performance.Cancel)// can't update tickets for a cancelled performance
            throw new InvalidOperationException("Performance cancelled cant update tickets.");
        _context.Tickets.Update(ticket);
        await _context.SaveChangesAsync();
        return ticket;
    }
    
    
    public async Task<IEnumerable<Ticket>> UpdateTicketsByBookingRefAsync(List<Ticket> tickets)
    {
        if (tickets == null || tickets.Count == 0)
            throw new ArgumentException("No tickets provided for update.");

        var performanceId = tickets.First().PerformanceId;  
       
        var performance = await _context.Performances.FindAsync(performanceId);
        if (performance != null && performance.Cancel)
            throw new InvalidOperationException("Performance cancelled cant update tickets.");

        foreach (var ticket in tickets)
        {
            ticket.Performance = null; // prevent EF from reattaching an already tracked instance
            ticket.User = null; // prevent EF from reattaching an already tracked instance
            ticket.ReservedSeats = null; // prevent EF from reattaching an already tracked instance
            _context.Tickets.Update(ticket);
        }

        await _context.SaveChangesAsync();
        return tickets;
    }

    public async Task<bool> DeleteTicketsByBookingRefAsync(string bookingRef)
    {
        _logger.LogInformation($"Deleting tickets with BookingRef: {bookingRef}");

        var tickets = await _context.Tickets
            .Where(t => t.QRInCode == bookingRef)
            .ToListAsync();

        if (!tickets.Any())
            return false;

        foreach (var ticket in tickets)
        {
            ticket.Cancelled = true;
            ticket.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteTicketAsync(int ticketId)
    {
        _logger.LogInformation($"Deleting ticket with ID: {ticketId}");
        var ticket = await _context.Tickets.FindAsync(ticketId);
        if (ticket == null)
            return false;

        // Mark as cancelled instead of physical deletion for audit purposes
        ticket.Cancelled = true;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> TicketExistsAsync(int ticketId)
    {
        return await _context.Tickets.AnyAsync(t => t.TicketId == ticketId);
    }
}
