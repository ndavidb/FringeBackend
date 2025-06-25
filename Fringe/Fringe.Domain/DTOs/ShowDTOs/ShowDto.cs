using Fringe.Domain.DTOs.TicketTypeDTOs;
using Fringe.Domain.DTOs.VenueDTOs;
using Fringe.Domain.Entities;

namespace Fringe.Domain.DTOs.ShowDTOs;

public class ShowDto
{
    public int ShowId { get; set; }
    public string ShowName { get; set; }
    public int VenueId { get; set; }
    public string VenueName { get; set; }
    public int ShowTypeId { get; set; }
    public string ShowType { get; set; }
    public string Description { get; set; }
    public int AgeRestrictionId { get; set; }
    public string AgeRestrictionCode { get; set; }
    public string WarningDescription { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? TicketTypeId { get; set; }
    public string TicketTypeName { get; set; }
    public string ImagesUrl { get; set; }
    public string VideosUrl { get; set; }
    public bool Active { get; set; }

    public VenueDto Venue { get; set; }
    public ShowTypeLookupDto ShowTypeLookup { get; set; }
    public TicketTypeDto TicketType { get; set; }
}