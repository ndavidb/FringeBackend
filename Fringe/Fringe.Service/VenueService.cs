using Fringe.Domain.DTOs.VenueDTOs;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Fringe.Service;

public class VenueService : IVenueService
{
    private readonly IVenueRepository _venueRepository;
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<VenueService> _logger;

    public VenueService(
        IVenueRepository venueRepository,
        IWebHostEnvironment webHostEnvironment,
        ILogger<VenueService> logger)
    {
        _venueRepository = venueRepository;
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    // Get venue list
    public async Task<IEnumerable<VenueDto>> GetAllVenuesAsync()
    {
        var venues = await _venueRepository.GetAllVenuesAsync();
        return venues.Select(MapToVenueDto);
    }

    // get venue by ID
    public async Task<VenueDto?> GetVenueByIdAsync(int id)
    {
        var venue = await _venueRepository.GetByIdAsync(id);
        return venue == null ? null : MapToVenueDto(venue);
    }

    // save a venue
    public async Task<VenueDto> CreateVenueAsync(CreateVenueDto dto, string creatorUserId)
    {
        if (await _venueRepository.VenueNameExistsAsync(dto.VenueName))
        {
            throw new InvalidOperationException($"Venue with name {dto.VenueName} already exists");
        }

        var guidUserId = Guid.Parse(creatorUserId);
        var creatorId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        var venue = MapToVenue(dto);
        venue.CreatedById = creatorId;
        venue.CreatedAt = DateTime.UtcNow;

        if ((dto.SeatsPerRow * dto.Rows) > dto.MaxCapacity)
        {
            throw new InvalidOperationException("The total number of seats exceeds the maximum capacity.");
        }

        var seatingPlan = new SeatingPlan
        {
            Rows = dto.Rows,
            SeatsPerRow = dto.SeatsPerRow,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        venue.SeatingPlan = seatingPlan;

        await _venueRepository.AddAsync(venue);
        return MapToVenueDto(venue);
    }

    public async Task<VenueDto> UpdateVenueAsync(int id, CreateVenueDto dto, string updaterUserId)
    {
        var existing = await _venueRepository.GetByIdAsync(id);
        if (existing == null)
            throw new InvalidOperationException($"Venue with ID {id} not found");

        // Check if image is being changed, if so, delete the old image
        if (!string.IsNullOrEmpty(existing.ImagesUrl) &&
            !string.IsNullOrEmpty(dto.ImagesUrl) &&
            existing.ImagesUrl != dto.ImagesUrl)
        {
            DeleteImageFile(existing.ImagesUrl);
        }
        
        if ((dto.SeatsPerRow * dto.Rows) > dto.MaxCapacity)
        {
            throw new InvalidOperationException("The total number of seats exceeds the maximum capacity.");
        }

        var guidUserId = Guid.Parse(updaterUserId);
        var updaterId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        // Manual mapping of properties
        UpdateVenueFromDto(existing, dto);
        existing.UpdatedId = updaterId;
        existing.UpdatedAt = DateTime.UtcNow;
        
        existing.SeatingPlan.Rows = dto.Rows;
        existing.SeatingPlan.SeatsPerRow = dto.SeatsPerRow;
        existing.SeatingPlan.UpdatedAt = DateTime.UtcNow;

        await _venueRepository.UpdateAsync(existing);
        return MapToVenueDto(existing);
    }

    public async Task<bool> DeleteVenueAsync(int id)
    {
        var venue = await _venueRepository.GetByIdAsync(id);
        if (venue == null)
            throw new InvalidOperationException($"Venue with ID {id} not found.");

        // Delete the associated image file if it exists
        if (!string.IsNullOrEmpty(venue.ImagesUrl))
        {
            DeleteImageFile(venue.ImagesUrl);
        }

        await _venueRepository.DeleteAsync(id);

        return true;
    }

    public async Task<IEnumerable<VenueTypeLookup>> GetVenueTypeLookUpAsync()
    {
        var venueTypes = await _venueRepository.GetVenueTypeLookUpAsync();
        return venueTypes;
    }

    private void DeleteImageFile(string imageUrl)
    {
        try
        {
            if (string.IsNullOrEmpty(imageUrl))
                return;

            // Normalize the path to remove any leading slash
            if (imageUrl.StartsWith("/"))
                imageUrl = imageUrl.Substring(1);

            // Get the physical file path
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, imageUrl);

            _logger.LogInformation($"Attempting to delete image file: {filePath}");

            // Check if file exists
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
                _logger.LogInformation($"Successfully deleted image file: {filePath}");
            }
            else
            {
                _logger.LogWarning($"Image file not found: {filePath}");
            }
        }
        catch (Exception ex)
        {
            // Log the error but don't throw exception to prevent stopping the venue deletion process
            _logger.LogError(ex, $"Error deleting image file: {imageUrl}");
        }
    }

    // Manual mapping methods
    private VenueDto MapToVenueDto(Venue venue)
    {
        return new VenueDto
        {
            VenueId = venue.VenueId,
            VenueName = venue.VenueName,
            TypeId = venue.TypeId,
            MaxCapacity = venue.MaxCapacity,
            Description = venue.Description,
            ContactEmail = venue.ContactEmail,
            ContactPhone = venue.ContactPhone,
            ImagesUrl = venue.ImagesUrl,
            Active = venue.Active,
            VenueUrl = venue.VenueUrl,
            LocationId = venue.LocationId,
            LocationName = venue.Location?.LocationName ?? string.Empty,
            SeatingPlanId = venue.SeatingPlan?.SeatingPlanId ?? 0,
            Rows = venue.SeatingPlan?.Rows ?? 0,
            SeatsPerRow = venue.SeatingPlan?.SeatsPerRow ?? 0
        };
    }

    private Venue MapToVenue(CreateVenueDto dto)
    {
        return new Venue
        {
            VenueName = dto.VenueName,
            TypeId = dto.TypeId,
            MaxCapacity = dto.MaxCapacity,
            Description = dto.Description,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            ImagesUrl = dto.ImagesUrl,
            Active = dto.Active,
            VenueUrl = dto.VenueUrl,
            LocationId = dto.LocationId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

// Implement the UpdateVenueFromDto method
    private void UpdateVenueFromDto(Venue venue, CreateVenueDto dto)
    {
        venue.VenueName = dto.VenueName;
        venue.TypeId = dto.TypeId;
        venue.MaxCapacity = dto.MaxCapacity;
        venue.Description = dto.Description;
        venue.ContactEmail = dto.ContactEmail;
        venue.ContactPhone = dto.ContactPhone;
        venue.ImagesUrl = dto.ImagesUrl;
        venue.Active = dto.Active;
        venue.VenueUrl = dto.VenueUrl;
        venue.LocationId = dto.LocationId;
    }
}