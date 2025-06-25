using System.Security.Claims;
using Fringe.Domain.DTOs.ShowDTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fringe.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class ShowsController : ControllerBase
{
    private readonly IShowService _showService;
    private readonly ILogger<ShowsController> _logger;

    public ShowsController(IShowService showService, ILogger<ShowsController> logger)
    {
        _showService = showService;
        _logger = logger;
    }

    // GET: api/shows
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllShows()
    {
        var shows = await _showService.GetAllShowsAsync();
        return Ok(shows);
    }

    // GET: api/shows/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShowById(int id)
    {
        var show = await _showService.GetShowByIdAsync(id);
        if (show == null)
            return NotFound();

        return Ok(show);
    }

    // POST: api/shows
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateShow([FromBody] CreateShowDto createShowDto)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var createdShow = await _showService.CreateShowAsync(createShowDto, creatorUserId);
            return CreatedAtAction(nameof(GetShowById), new { id = createdShow.ShowId }, createdShow);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating show");
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/shows/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdateShow(int id, [FromBody] UpdateShowDto updateShowDto)
    {
        try
        {
            var updaterUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var updatedShow = await _showService.UpdateShowAsync(id, updateShowDto, updaterUserId);
            return Ok(updatedShow);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating show");
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/shows/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeleteShow(int id)
    {
        try
        {
            await _showService.DeleteShowAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return NotFound(ex.Message);
        }
    }

    // GET: api/shows/age-restrictions
    [HttpGet("age-restrictions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAgeRestrictions()
    {
        var ageRestrictions = await _showService.GetAllAgeRestrictionsAsync();
        return Ok(ageRestrictions);
    }

    // GET: api/shows/show-types
    [HttpGet("show-types")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetShowTypes()
    {
        var showTypes = await _showService.GetAllShowTypesAsync();
        return Ok(showTypes);
    }
}