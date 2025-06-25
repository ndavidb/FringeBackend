using Fringe.Domain.DTOs.ShowDTOs;
using Fringe.Domain.DTOs.TicketTypeDTOs;
using Fringe.Domain.DTOs.VenueDTOs;

namespace Fringe.Service;

public class ShowService : IShowService
{
    private readonly IShowRepository _showRepository;

    public ShowService(IShowRepository showRepository)
    {
        _showRepository = showRepository;
    }

    public async Task<IEnumerable<ShowDto>> GetAllShowsAsync()
    {
        var shows = await _showRepository.GetAllShowsAsync();
        return shows.Select(s => MapToDto(s));
    }

    public async Task<ShowDto> GetShowByIdAsync(int showId)
    {
        var show = await _showRepository.GetShowByIdAsync(showId);
        if (show == null)
            return null;

        return MapToDto(show);
    }

    public async Task<ShowDto> CreateShowAsync(CreateShowDto createShowDto, string creatorUserId)
    {
        if (await _showRepository.ShowNameExistsAsync(createShowDto.ShowName))
        {
            throw new InvalidOperationException($"show with name {createShowDto.ShowName} already exists");
        }

        var guidUserId = Guid.Parse(creatorUserId);
        var creatorId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        var show = new Show
        {
            ShowName = createShowDto.ShowName,
            VenueId = createShowDto.VenueId,
            ShowTypeId = createShowDto.ShowTypeId,
            Description = createShowDto.Description,
            AgeRestrictionId = createShowDto.AgeRestrictionId,
            WarningDescription = createShowDto.WarningDescription,
            StartDate = createShowDto.StartDate,
            EndDate = createShowDto.EndDate,
            TicketTypeId = createShowDto.TicketTypeId,
            ImagesUrl = createShowDto.ImagesUrl,
            VideosUrl = createShowDto.VideosUrl,
            Active = true,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        var createdShow = await _showRepository.CreateShowAsync(show);
        return MapToDto(await _showRepository.GetShowByIdAsync(createdShow.ShowId));
    }

    public async Task<ShowDto> UpdateShowAsync(int showId, UpdateShowDto updateShowDto, string updaterUserId)
    {
        var existingShow = await _showRepository.GetShowByIdAsync(showId);
        if (existingShow == null)
            throw new InvalidOperationException($"Show with ID {showId} not found");

        var guidUserId = Guid.Parse(updaterUserId);
        var updaterId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        existingShow.ShowName = updateShowDto.ShowName;
        existingShow.VenueId = updateShowDto.VenueId;
        existingShow.ShowTypeId = updateShowDto.ShowTypeId;
        existingShow.Description = updateShowDto.Description;
        existingShow.AgeRestrictionId = updateShowDto.AgeRestrictionId;
        existingShow.WarningDescription = updateShowDto.WarningDescription;
        existingShow.StartDate = updateShowDto.StartDate;
        existingShow.EndDate = updateShowDto.EndDate;
        existingShow.TicketTypeId = updateShowDto.TicketTypeId;
        existingShow.ImagesUrl = updateShowDto.ImagesUrl;
        existingShow.VideosUrl = updateShowDto.VideosUrl;
        existingShow.Active = updateShowDto.Active;
        existingShow.UpdatedId = updaterId;
        existingShow.UpdatedAt = DateTime.UtcNow;

        var updatedShow = await _showRepository.UpdateShowAsync(existingShow);
        return MapToDto(await _showRepository.GetShowByIdAsync(updatedShow.ShowId));
    }

    public async Task<bool> DeleteShowAsync(int showId)
    {
        if (!await _showRepository.ShowExistsAsync(showId))
            throw new InvalidOperationException($"Show with ID {showId} not found");

        return await _showRepository.DeleteShowAsync(showId);
    }

    public async Task<IEnumerable<AgeRestrictionDto>> GetAllAgeRestrictionsAsync()
    {
        var ageRestrictions = await _showRepository.GetAllAgeRestrictionsAsync();
        return ageRestrictions.Select(ar => new AgeRestrictionDto
        {
            AgeRestrictionId = ar.AgeRestrictionId,
            Code = ar.Code,
            Description = ar.Description
        });
    }

    public async Task<IEnumerable<ShowTypeLookupDto>> GetAllShowTypesAsync()
    {
        var showTypes = await _showRepository.GetAllShowTypesAsync();
        return showTypes.Select(st => new ShowTypeLookupDto
        {
            TypeId = st.TypeId,
            ShowType = st.ShowType
        });
    }

    private static ShowDto MapToDto(Show show)
    {
#pragma warning disable CS8601 // Possible null reference assignment.
        return new ShowDto
        {
            ShowId = show.ShowId,
            ShowName = show.ShowName,
            VenueId = show.VenueId,
            VenueName = show.Venue?.VenueName,
            ShowTypeId = show.ShowTypeId,
            ShowType = show.ShowTypeLookup?.ShowType,
            Description = show.Description,
            AgeRestrictionId = show.AgeRestrictionId,
            AgeRestrictionCode = show.AgeRestrictionLookup?.Code,
            WarningDescription = show.WarningDescription,
            StartDate = show.StartDate,
            EndDate = show.EndDate,
            TicketTypeId = show.TicketTypeId,
            TicketTypeName = show.TicketType?.TypeName,
            ImagesUrl = show.ImagesUrl,
            VideosUrl = show.VideosUrl,
            Active = show.Active,

            // Complete mapping for Venue navigation property
            Venue = show.Venue != null ? new VenueDto
            {
                VenueId = show.Venue.VenueId,
                VenueName = show.Venue.VenueName,
                TypeId = show.Venue.TypeId,
                MaxCapacity = show.Venue.MaxCapacity,
                Description = show.Venue.Description,
                ContactEmail = show.Venue.ContactEmail,
                ContactPhone = show.Venue.ContactPhone,
                ImagesUrl = show.Venue.ImagesUrl,
                Active = show.Venue.Active,
                VenueUrl = show.Venue.VenueUrl,
                LocationId = show.Venue.LocationId,
                // Map SeatingPlan if it exists
                SeatingPlan = show.Venue.SeatingPlan != null ? new SeatingPlanDto
                {
                    SeatingPlanId = show.Venue.SeatingPlan.SeatingPlanId,
                    VenueId = show.Venue.SeatingPlan.VenueId,
                    Rows = show.Venue.SeatingPlan.Rows,
                    SeatsPerRow = show.Venue.SeatingPlan.SeatsPerRow
                } : null
            } : null,

            // Map ShowTypeLookup
            ShowTypeLookup = show.ShowTypeLookup != null ? new ShowTypeLookupDto
            {
                TypeId = show.ShowTypeLookup.TypeId,
                ShowType = show.ShowTypeLookup.ShowType
                // Add other properties as needed for ShowTypeLookupDto
            } : null,

            // Map TicketType
            TicketType = show.TicketType != null ? new TicketTypeDto
            {
                TicketTypeId = show.TicketType.TicketTypeId,
                TypeName = show.TicketType.TypeName
                // Add other properties as needed for TicketTypeDto
            } : null
        };
#pragma warning restore CS8601 // Possible null reference assignment.
    }
}