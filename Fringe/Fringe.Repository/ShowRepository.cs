namespace Fringe.Repository;

public class ShowRepository : IShowRepository
{
    private readonly FringeDbContext _context;

    public ShowRepository(FringeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Show>> GetAllShowsAsync()
    {
        return await _context.Shows
            .AsNoTracking()
            .Include(s => s.Venue)
            .Include(s => s.ShowTypeLookup)
            .Include(s => s.AgeRestrictionLookup)
            .Include(s => s.TicketType)
            .ToListAsync();
    }

    public async Task<Show> GetShowByIdAsync(int showId)
    {
        return await _context.Shows
            .Include(s => s.Venue)
            .Include(s => s.ShowTypeLookup)
            .Include(s => s.AgeRestrictionLookup)
            .Include(s => s.TicketType)
            .Include(s => s.Venue.SeatingPlan)
            .FirstOrDefaultAsync(s => s.ShowId == showId) ?? throw new InvalidOperationException("Show ID must not be null");
    }

    public async Task<Show> CreateShowAsync(Show show)
    {
        await _context.Shows.AddAsync(show);
        await _context.SaveChangesAsync();
        return show;
    }

    public async Task<Show> UpdateShowAsync(Show show)
    {
        _context.Shows.Update(show);
        await _context.SaveChangesAsync();
        return show;
    }

    public async Task<bool> DeleteShowAsync(int showId)
    {
        var show = await _context.Shows.FindAsync(showId);
        if (show == null)
            return false;

        _context.Shows.Remove(show);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> ShowExistsAsync(int showId)
    {
        return await _context.Shows.AnyAsync(s => s.ShowId == showId);
    }

    public async Task<bool> ShowNameExistsAsync(string showName)
    {
        return await _context.Shows.Where(s => s.ShowName.ToLower() == showName.ToLower()).AnyAsync();
    }

    public async Task<IEnumerable<AgeRestrictionLookup>> GetAllAgeRestrictionsAsync()
    {
        return await _context.AgeRestrictionLookups.ToListAsync();
    }

    public async Task<IEnumerable<ShowTypeLookup>> GetAllShowTypesAsync()
    {
        return await _context.ShowTypeLookups.ToListAsync();
    }
}