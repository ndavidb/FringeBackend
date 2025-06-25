using System.Security.Claims;
using Fringe.Domain.DTOs;
using Fringe.Domain.DTOs.VenueDTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fringe.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class VenuesController : ControllerBase
{
    private readonly IVenueService _venueService;
    private readonly ILogger<VenuesController> _logger;

    public VenuesController(IVenueService venueService, ILogger<VenuesController> logger)
    {
        _venueService = venueService;
        _logger = logger;
    }
    

    /**Retrieves a list of all venues along with their related location and type information.
    <returns>List of VenueDto</returns> **/
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllVenues()
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("User {UserId} - Getting all venues", creatorUserId);
        var venues = await _venueService.GetAllVenuesAsync();
        return Ok(venues);
    }
    
    
    // Gets a venue by ID
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVenueById(int id)
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _logger.LogInformation("User {UserId} - Getting venue by id {id} ", creatorUserId, id);
        var venue = await _venueService.GetVenueByIdAsync(id);
        if (venue == null)
            return NotFound();
        return Ok(venue);
    }
    
    
    // POST - Save a venue record
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateVenue([FromBody] CreateVenueDto dto)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Saving venue", creatorUserId);
            
            if (string.IsNullOrEmpty(creatorUserId))
            {
                return BadRequest("User ID claim is missing");
            }
            
            var createVenue = await _venueService.CreateVenueAsync(dto, creatorUserId);
            _logger.LogInformation("User {UserId} - Getting venue by id ", creatorUserId);
            return CreatedAtAction(nameof(GetVenueById), new { id = createVenue.VenueId }, createVenue);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating venue");
            return BadRequest(ex.Message);
        }
       
    }
    
    
    //PUT - Updates a venue by ID.</summary>
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateVenue(int id, [FromBody] CreateVenueDto dto)
    {
        try
        {
            var updaterUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Update venue", updaterUserId);
            var updatedVenue = await _venueService.UpdateVenueAsync(id, dto, updaterUserId);
            return Ok(updatedVenue);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error updating venue");
            return BadRequest(ex.Message);
        }
    }
    
    
    
    //Deletes a venue by ID
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteVenue(int id)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Deleting venue by id {id} ", creatorUserId, id);
            await _venueService.DeleteVenueAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error deleting venue");
            return NotFound(ex.Message);
        }
    }

    [HttpGet("venue-types")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetVenueType()
    {
        try
        {
            _logger.LogInformation("Getting venue type");
            var venueTypes = await _venueService.GetVenueTypeLookUpAsync();
            return Ok(venueTypes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,"Error getting venue type");
            return NotFound(ex.Message);
        }

    }
    
    
    
    
    
    
}