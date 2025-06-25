using Microsoft.Extensions.Logging;

namespace Fringe.Repository;

public class PerformanceRepository  : IPerformanceRepository
{
     private readonly FringeDbContext _context;
    private readonly ILogger<PerformanceRepository> _logger;

    public PerformanceRepository(FringeDbContext context, ILogger<PerformanceRepository> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<IEnumerable<Performance>> GetAllPerformancesAsync()
    {
        return await _context.Performances
            .Include(p => p.Show)
            .ThenInclude(s => s.Venue)
            .Include(p => p.TicketPrices)!
            .ThenInclude(tp => tp.TicketType)
            .Include(p => p.Tickets)
            .ThenInclude(t => t.ReservedSeats)
            .OrderBy(p => p.PerformanceDate)
            .ThenBy(p => p.StartTime)
            .ToListAsync();
    }

    public async Task<IEnumerable<Performance>> GetPerformancesByShowIdAsync(int showId)
    {
        return await _context.Performances
            .Include(p => p.Show)
            .ThenInclude(s => s.Venue)
            .ThenInclude(v => v.SeatingPlan)  
            .Include(p => p.TicketPrices)!
            .ThenInclude(tp => tp.TicketType)
            .Include(p => p.Tickets)
            .ThenInclude(t => t.ReservedSeats)
            .Where(p => p.ShowId == showId)
            .OrderBy(p => p.PerformanceDate)
            .ThenBy(p => p.StartTime)
            .ToListAsync();
    }

    public async Task<Performance?> GetPerformanceByIdAsync(int performanceId)
    {
        return await _context.Performances
            .Include(p => p.Show)
            .ThenInclude(s => s.Venue)
            .ThenInclude(v => v.SeatingPlan)
            .Include(p => p.TicketPrices)!
            .ThenInclude(tp => tp.TicketType)
            .Include(p => p.Tickets)
            .ThenInclude(t => t.ReservedSeats)
            .FirstOrDefaultAsync(p => p.PerformanceId == performanceId);
    }

    public async Task<Performance> CreatePerformanceAsync(Performance performance)
    {
        await _context.Performances.AddAsync(performance);
        await _context.SaveChangesAsync();
        return performance;
    }

    public async Task<IEnumerable<Performance>> CreateBatchPerformancesAsync(IEnumerable<Performance> performances)
    {
        await _context.Performances.AddRangeAsync(performances);
        await _context.SaveChangesAsync();
        return performances;
    }

    public async Task<Performance> UpdatePerformanceAsync(Performance performance)
    {
        _context.Performances.Update(performance);
        await _context.SaveChangesAsync();
        return performance;
    }

    public async Task<bool> DeletePerformanceAsync(int performanceId)
    {
        var performance = await _context.Performances.FindAsync(performanceId);
        if (performance == null)
            return false;

        // Check if there are any tickets associated with this performance
        var hasTickets = await _context.Tickets.AnyAsync(t => t.PerformanceId == performanceId);
        
        if (hasTickets)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            
            try
            {
                performance.Cancel = true;
                performance.Active = false;
                performance.UpdatedAt = DateTime.UtcNow;


                var tickets = await _context.Tickets.Where(t => t.PerformanceId == performanceId && !t.Cancelled)
                    .ToListAsync();

                _logger.LogInformation("Cancelling performance {PerformanceId} with {TicketCount} tickets",
                    performanceId, tickets.Count);
                foreach (var ticket in tickets)
                {
                    ticket.Cancelled = true;
                    ticket.UpdatedAt = DateTime.UtcNow;
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
                return true;
            } 
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error cancelling performance {PerformanceId}", performanceId);
                await transaction.RollbackAsync();
                throw;
            }
        }

        _context.Performances.Remove(performance);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> PerformanceExistsAsync(int performanceId)
    {
        return await _context.Performances.AnyAsync(p => p.PerformanceId == performanceId);
    }

    public async Task<bool> HasConflictingPerformancesAsync(int showId, DateTime performanceDate, TimeSpan startTime, TimeSpan endTime, int? excludePerformanceId = null)
    {
        
        var show = await _context.Shows.FindAsync(showId);
        if (show == null)
            return false; // Show doesn't exist, so we can't check for conflicts
            
        int venueId = show.VenueId;
        
        
        var query = _context.Performances
            .Include(p => p.Show)
            .Where(p => p.Show.VenueId == venueId && 
                       p.PerformanceDate.Date == performanceDate.Date &&
                       p.Active && 
                       !p.Cancel);
        
        if (excludePerformanceId.HasValue)
        {
            query = query.Where(p => p.PerformanceId != excludePerformanceId.Value);
        }
        
        return await query.AnyAsync(p => 
            (startTime < p.EndTime && endTime > p.StartTime));
    }
}