namespace Fringe.Repository.Interfaces;

public interface IShowRepository
{
    Task<IEnumerable<Show>> GetAllShowsAsync();
    Task<Show> GetShowByIdAsync(int showId);
    Task<Show> CreateShowAsync(Show show);
    Task<Show> UpdateShowAsync(Show show);
    Task<bool> DeleteShowAsync(int showId);
    Task<bool> ShowExistsAsync(int showId);
    Task<bool> ShowNameExistsAsync(string showName);
    Task<IEnumerable<AgeRestrictionLookup>> GetAllAgeRestrictionsAsync();
    Task<IEnumerable<ShowTypeLookup>> GetAllShowTypesAsync();
}