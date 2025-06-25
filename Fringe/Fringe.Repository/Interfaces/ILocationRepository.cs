namespace Fringe.Repository.Interfaces;

public interface ILocationRepository
{
    Task<List<Location>> GetAllAsync();
    Task<Location?> GetByIdAsync(int id);
    Task<Location> AddAsync(Location location);
    Task UpdateAsync(Location location);
    Task<bool> DeleteAsync(int locationId);
    Task<bool> LocationNameExistsAsync(string locationName);
    Task<bool> LocationExistsAsync(int id);
}