using Fringe.Domain.DTOs.VenueDTOs;

namespace Fringe.Service.Interfaces;

public interface IVenueService
{
    Task<IEnumerable<VenueDto>> GetAllVenuesAsync();
    
    Task<VenueDto?> GetVenueByIdAsync(int id);
    
    Task<VenueDto> CreateVenueAsync(CreateVenueDto dto, string creatorUserId);
    
    Task<VenueDto> UpdateVenueAsync(int id, CreateVenueDto dto, string updaterUserId);
    
    Task<bool> DeleteVenueAsync(int id);
    Task<IEnumerable<VenueTypeLookup>> GetVenueTypeLookUpAsync();
}