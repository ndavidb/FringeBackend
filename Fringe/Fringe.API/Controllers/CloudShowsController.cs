using Fringe.Domain;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Fringe.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class CloudShowsController : ControllerBase
{
    private readonly ILogger<CloudShowsController> _logger;
    private readonly FringeDbContext _context;
    
    public CloudShowsController(ILogger<CloudShowsController> logger, FringeDbContext context)
    {
        _logger = logger;
        _context = context;
    }
    
    [HttpGet]
    [Route("Cloud Get All Shows")]
    public async Task<IActionResult> GetCloudShows()
    {
        try
        {
            var shows = await _context.Shows.ToListAsync();
            if (shows.Count == 0)
            {
                return NotFound("No shows for your location");
            }
            return Ok(shows);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shows for the cloud");
            return StatusCode(500, "Internal server error");
        }
    }
    
    [HttpGet]
    [Route("GetShowsByPostalCodes")]
    public async Task<IActionResult> GetShowsByPostalCodes([FromQuery] List<string> postalCodes)
    {
        try
        {
            if (postalCodes == null || postalCodes.Count == 0)
            {
                return BadRequest("At least one postal code is required");
            }
            
            var shows = await _context.Shows
                .Include(s => s.ShowTypeLookup)
                .Include(s => s.Venue)
                .ThenInclude(v => v.Location)
                .Where(s => postalCodes.Contains(s.Venue.Location.PostalCode)
                            && s.Active)
                .ToListAsync();
                
            if (!shows.Any())
            {
                return NotFound($"No shows found for the provided postal codes: {string.Join(", ", postalCodes)}");
            }

            var result = shows.Select(s => new
            {
                s.ShowId,
                s.ShowName,
                s.ShowTypeLookup.ShowType,
                s.Description,
                s.StartDate,
                s.EndDate,
                s.Venue.VenueName,
                s.Venue.Location.Address,
                s.Venue.Location.Suburb,
                s.Venue.Location.PostalCode,
                s.Venue.Location.State,
                s.Venue.Location.ParkingAvailable
            }).ToList();
            
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving shows for postal codes: {PostalCodes}", string.Join(", ", postalCodes));
            return StatusCode(500, "Internal server error");
        }
    }
}