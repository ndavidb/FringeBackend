using Fringe.Domain.DTOs.ShowDTOs;

namespace Fringe.Service.Interfaces;

public interface IShowService
{
    Task<IEnumerable<ShowDto>> GetAllShowsAsync();
    Task<ShowDto> GetShowByIdAsync(int showId);
    Task<ShowDto> CreateShowAsync(CreateShowDto createShowDto, string creatorUserId);
    Task<ShowDto> UpdateShowAsync(int showId, UpdateShowDto updateShowDto, string updaterUserId);
    Task<bool> DeleteShowAsync(int showId);
    Task<IEnumerable<AgeRestrictionDto>> GetAllAgeRestrictionsAsync();
    Task<IEnumerable<ShowTypeLookupDto>> GetAllShowTypesAsync();
}