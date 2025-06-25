namespace Fringe.Domain.Entities;

public class Show
{
    public int ShowId { get; set; }
    public string ShowName { get; set; }
    public int VenueId { get; set; }
    public int ShowTypeId { get; set; }
    public string Description { get; set; }
    public int AgeRestrictionId { get; set; }
    public string WarningDescription { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public int? TicketTypeId { get; set; }
    public string ImagesUrl { get; set; }
    public string VideosUrl { get; set; }
    public bool Active { get; set; } = true;
    public int CreatedById { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public int? UpdatedId { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public Venue Venue { get; set; }
    public ShowTypeLookup ShowTypeLookup { get; set; }
    public AgeRestrictionLookup AgeRestrictionLookup { get; set; }
    public TicketType TicketType { get; set; }
}