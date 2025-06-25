using Fringe.Domain.DTOs.LocationDTOs;

namespace Fringe.Service.Interfaces;

public interface ILocationService
{
    Task<List<LocationDto>> GetAllLocationsAsync();
    Task<LocationDto?> GetLocationByIdAsync(int id);
    Task<LocationDto> CreateLocationAsync(CreateLocationDto dto, string updaterUserId);
    Task<LocationDto> UpdateLocationAsync(int id, CreateLocationDto dto, string updaterUserId);
    Task DeleteLocationAsync(int id);
}