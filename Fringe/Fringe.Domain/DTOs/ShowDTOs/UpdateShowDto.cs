using System.ComponentModel.DataAnnotations;

namespace Fringe.Domain.DTOs.ShowDTOs;

public class UpdateShowDto
{
    [Required]
    [StringLength(150)]
    public string ShowName { get; set; }
    
    [Required]
    public int VenueId { get; set; }
    
    [Required]
    public int ShowTypeId { get; set; }
    
    public string Description { get; set; }
    
    [Required]
    public int AgeRestrictionId { get; set; }
    
    public string WarningDescription { get; set; }
    
    [Required]
    public DateTime StartDate { get; set; }
    
    [Required]
    public DateTime EndDate { get; set; }
    
    public int? TicketTypeId { get; set; }
    
    public string ImagesUrl { get; set; }
    
    public string VideosUrl { get; set; }
    
    public bool Active { get; set; }
}