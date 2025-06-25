using Fringe.Domain.DTOs.LocationDTOs;

namespace Fringe.Service;

public class LocationService : ILocationService
{
    private readonly ILocationRepository _locationRepository;

    public LocationService(ILocationRepository locationRepository)
    {
        _locationRepository = locationRepository;
    }

    public async Task<List<LocationDto>> GetAllLocationsAsync()
    {
        var locations = await _locationRepository.GetAllAsync();
        return locations.Select(MapToDto).ToList();
    }

    public async Task<LocationDto?> GetLocationByIdAsync(int id)
    {
        var location = await _locationRepository.GetByIdAsync(id);

        return location == null ? null : // <-- no throw, just return null as need to filter not found
            MapToDto(location);
    }

    public async Task<LocationDto> CreateLocationAsync(CreateLocationDto dto, string updaterUserId)
    {
        if (await _locationRepository.LocationNameExistsAsync(dto.LocationName))
        {
            throw new InvalidOperationException($"Location with name {dto.LocationName} already exists");
        }

        
        var guidUserId = Guid.Parse(updaterUserId);
        var updaterId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo
        
        var location = new Location
        {
            LocationName = dto.LocationName,
            Address = dto.Address,
            Suburb = dto.Suburb,
            PostalCode = dto.PostalCode,
            State = dto.State,
            Country = dto.Country,
            Latitude = dto.Latitude,
            Longitude = dto.Longitude,
            ParkingAvailable = dto.ParkingAvailable,
            CreatedAt = DateTime.UtcNow,
            Active = true,
            CreatedById = updaterId
        };

        var created = await _locationRepository.AddAsync(location);
        return MapToDto(created);
    }

    public async Task<LocationDto> UpdateLocationAsync(int id, CreateLocationDto dto, string updaterUserId)
    {
        var location = await _locationRepository.GetByIdAsync(id)
                       ?? throw new InvalidOperationException($"Location with ID {id} not found");
        
        var guidUserId = Guid.Parse(updaterUserId);
        var updaterId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        location.LocationName = dto.LocationName;
        location.Address = dto.Address;
        location.Suburb = dto.Suburb;
        location.PostalCode = dto.PostalCode;
        location.State = dto.State;
        location.Country = dto.Country;
        location.Latitude = dto.Latitude;
        location.Longitude = dto.Longitude;
        location.ParkingAvailable = dto.ParkingAvailable;
        location.UpdatedAt = DateTime.UtcNow;
        location.UpdatedId = updaterId;
        location.Active = dto.Active;

        await _locationRepository.UpdateAsync(location);
        return MapToDto(location);
    }

    public async Task DeleteLocationAsync(int id)
    {
        if (!await _locationRepository.LocationExistsAsync(id))
            throw new InvalidOperationException($"Location with ID {id} not found");
            
        await _locationRepository.DeleteAsync(id);
    }

    private LocationDto MapToDto(Location l) => new()
    {
        LocationId = l.LocationId,
        LocationName = l.LocationName,
        Address = l.Address,
        Suburb = l.Suburb,
        PostalCode = l.PostalCode,
        State = l.State,
        Country = l.Country,
        Latitude = l.Latitude,
        Longitude = l.Longitude,
        ParkingAvailable = l.ParkingAvailable,
        Active = l.Active
    };
}