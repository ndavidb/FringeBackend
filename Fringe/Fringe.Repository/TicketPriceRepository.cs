using Microsoft.Extensions.Logging;

namespace Fringe.Repository;

public class TicketPriceRepository : ITicketPriceRepository
{
   private readonly FringeDbContext _context;
    private readonly ILogger<TicketPriceRepository> _logger;

    public TicketPriceRepository(FringeDbContext context, ILogger<TicketPriceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<TicketPrice>> GetAllTicketPricesAsync()
    {
        _logger.LogInformation("Fetching all ticket prices");
        return await _context.TicketPrices
            .Include(tp => tp.TicketType)
            .Include(tp => tp.Performance)
            .ToListAsync();
    }

    public async Task<TicketPrice?> GetTicketPriceByIdAsync(int ticketPriceId)
    {
        _logger.LogInformation($"Fetching ticket price with ID: {ticketPriceId}");
        return await _context.TicketPrices
            .Include(tp => tp.TicketType)
            .Include(tp => tp.Performance)
            .FirstOrDefaultAsync(tp => tp.TicketPriceId == ticketPriceId);
    }

    public async Task<IEnumerable<TicketPrice>> GetTicketPricesByPerformanceIdAsync(int performanceId)
    {
        _logger.LogInformation($"Fetching ticket prices for performance ID: {performanceId}");
        return await _context.TicketPrices
            .Include(tp => tp.TicketType)
            .Where(tp => tp.PerformanceId == performanceId)
            .ToListAsync();
    }

    public async Task<IEnumerable<TicketPrice>> GetTicketPricesByTicketTypeIdAsync(int ticketTypeId)
    {
        _logger.LogInformation($"Fetching ticket prices for ticket type ID: {ticketTypeId}");
        return await _context.TicketPrices
            .Include(tp => tp.Performance)
            .Where(tp => tp.TicketTypeId == ticketTypeId)
            .ToListAsync();
    }

    public async Task<TicketPrice> CreateTicketPriceAsync(TicketPrice ticketPrice)
    {
        _logger.LogInformation($"Creating new ticket price for performance ID: {ticketPrice.PerformanceId} and ticket type ID: {ticketPrice.TicketTypeId}");
        await _context.TicketPrices.AddAsync(ticketPrice);
        await _context.SaveChangesAsync();
        return ticketPrice;
    }

    public async Task<TicketPrice> UpdateTicketPriceAsync(TicketPrice ticketPrice)
    {
        _logger.LogInformation($"Updating ticket price with ID: {ticketPrice.TicketPriceId}");
        _context.TicketPrices.Update(ticketPrice);
        await _context.SaveChangesAsync();
        return ticketPrice;
    }

    public async Task<bool> DeleteTicketPriceAsync(int ticketPriceId)
    {
        _logger.LogInformation($"Deleting ticket price with ID: {ticketPriceId}");
        var ticketPrice = await _context.TicketPrices.FindAsync(ticketPriceId);
        if (ticketPrice == null)
            return false;

        // Check if the ticket price is associated with a performance
        if (await IsAssociatedWithPerformanceAsync(ticketPriceId))
        {
            _logger.LogWarning($"Ticket price ID {ticketPriceId} is associated with a performance and cannot be deleted directly.");
            throw new InvalidOperationException($"Ticket price ID {ticketPriceId} is associated with a performance and cannot be deleted directly. Delete the performance instead.");
        }

        _context.TicketPrices.Remove(ticketPrice);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> TicketPriceExistsAsync(int ticketPriceId)
    {
        return await _context.TicketPrices.AnyAsync(tp => tp.TicketPriceId == ticketPriceId);
    }

    public async Task<bool> IsAssociatedWithPerformanceAsync(int ticketPriceId)
    {
        var ticketPrice = await _context.TicketPrices
            .AsNoTracking()
            .FirstOrDefaultAsync(tp => tp.TicketPriceId == ticketPriceId);
        
        return ticketPrice != null && ticketPrice.PerformanceId > 0;
    }
}