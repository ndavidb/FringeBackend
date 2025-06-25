namespace Fringe.Repository;

// Handles Venue data access logic
public class VenueRepository : IVenueRepository
{
    private readonly FringeDbContext _context;

    public VenueRepository(FringeDbContext context)
    {
        _context = context;
    }

    // Retrieves all venues and includes associated Location data
    public async Task<IEnumerable<Venue>> GetAllVenuesAsync()
    {
        return await _context.Venues
            .Include(v => v.Location)
            .Include(v => v.SeatingPlan)
            .Include(v => v.VenueTypeLookUp)
            .ToListAsync();
    }
    
    // Retrieves a venue by id
    public async Task<Venue?> GetByIdAsync(int id)
    {
        return await _context.Venues
            .Include(v => v.Location)
            .Include(v => v.SeatingPlan)
            .Include(v => v.VenueTypeLookUp)
            .FirstOrDefaultAsync(v => v.VenueId == id);
    }

    public async Task AddAsync(Venue venue)
    {
        await _context.Venues.AddAsync(venue);
        await _context.SaveChangesAsync();
    }
    
    public async Task UpdateAsync(Venue venue)
    {
        _context.Venues.Update(venue);
        await _context.SaveChangesAsync();
    }
    
    
    public async Task DeleteAsync(int id)
    {
        var venue = await _context.Venues.FindAsync(id);
        if (venue == null)
            throw new InvalidOperationException("Venue not found");
        
        var hasShows = await _context.Shows.AnyAsync(s => s.VenueId == id);
        
        if (hasShows)
        {
            throw new InvalidOperationException("Cannot delete venue because it is associated with one or more shows.");
        }
        
        _context.Venues.Remove(venue);
        await _context.SaveChangesAsync();
        
    }

    public async Task<bool> VenueNameExistsAsync(string venueName)
    {
        return await _context.Venues.Where(s => s.VenueName.ToLower() == venueName.ToLower()).AnyAsync();
        
    }

    public async Task<IEnumerable<VenueTypeLookup>> GetVenueTypeLookUpAsync()
    {
        return await _context.VenueTypeLookUps.ToListAsync(); 
    }
}