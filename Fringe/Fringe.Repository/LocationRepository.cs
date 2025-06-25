namespace Fringe.Repository;

public class LocationRepository : ILocationRepository
{
    private readonly FringeDbContext _context;

    public LocationRepository(FringeDbContext context)
    {
        _context = context;
    }
    
    // retrieve all locations
    public async Task<List<Location>> GetAllAsync() =>
        await _context.Locations.ToListAsync();

    // get location by id
    public async Task<Location?> GetByIdAsync(int id) =>
        await _context.Locations.FindAsync(id);
    
    // save location
    public async Task<Location> AddAsync(Location location)
    {
        _context.Locations.Add(location);
        await _context.SaveChangesAsync();
        return location;
    }

    // update location
    public async Task UpdateAsync(Location location)
    {
        _context.Locations.Update(location);
        await _context.SaveChangesAsync();
    }

    // delete location
    public async Task<bool> DeleteAsync(int id)
    {
        var location = await _context.Locations.FindAsync(id);
        if (location == null)
            return false;

        var hasVenues = await _context.Venues.AnyAsync(v => v.LocationId == id);
        if (hasVenues)
        {
            throw new InvalidOperationException("Cannot delete location because it is associated with one or more venues.");
        }

        _context.Locations.Remove(location);

        await _context.SaveChangesAsync();
        return true;
    }
    
    public async Task<bool> LocationNameExistsAsync(string locationName)
    {
        return await _context.Locations.Where(s => s.LocationName.ToLower() == locationName.ToLower()).AnyAsync();
    }
    
    public async Task<bool> LocationExistsAsync(int id)
    {
        return await _context.Locations.AnyAsync(s => s.LocationId == id);
    }
    
}