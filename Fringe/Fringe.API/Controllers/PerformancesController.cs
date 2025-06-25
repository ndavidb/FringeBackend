using System.Security.Claims;
using Fringe.Domain.DTOs.PerformanceDTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fringe.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class PerformancesController : ControllerBase
{
    private readonly IPerformanceService _performanceService;
    private readonly ILogger<PerformancesController> _logger;

    public PerformancesController(
        IPerformanceService performanceService,
        ILogger<PerformancesController> logger)
    {
        _performanceService = performanceService;
        _logger = logger;
    }

    // GET: api/performances
    [HttpGet]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllPerformances()
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        _logger.LogInformation("User {UserId} - Getting all performances", creatorUserId);
        var performances = await _performanceService.GetAllPerformancesAsync();
        return Ok(performances);
    }

    // GET: api/performances/show/5
    [HttpGet("show/{showId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPerformancesByShowId(int showId)
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        _logger.LogInformation("User {UserId} - Getting performances for show ID: {ShowId}", creatorUserId, showId);
        var performances = await _performanceService.GetPerformancesByShowIdAsync(showId);
        return Ok(performances);
    }

    // GET: api/performances/5
    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPerformanceById(int id)
    {
        var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "Anonymous";
        _logger.LogInformation("User {UserId} - Getting performance by id {id}", creatorUserId, id);
        var performance = await _performanceService.GetPerformanceByIdAsync(id);
        if (performance == null)
            return NotFound();
        return Ok(performance);
    }

    // POST: api/performances
    [HttpPost]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreatePerformance([FromBody] CreatePerformanceDto dto)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Creating performance", creatorUserId);
            
            if (string.IsNullOrEmpty(creatorUserId))
            {
                return BadRequest("User ID claim is missing");
            }
            
            var createdPerformance = await _performanceService.CreatePerformanceAsync(dto, creatorUserId);
            return CreatedAtAction(nameof(GetPerformanceById), new { id = createdPerformance.PerformanceId }, createdPerformance);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating performance");
            return BadRequest(ex.Message);
        }
    }

    // POST: api/performances/batch
    [HttpPost("batch")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateBatchPerformances([FromBody] BatchCreatePerformanceDto dto)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Creating batch of {Count} performances", 
                creatorUserId, dto.PerformanceDates.Count);
            
            if (string.IsNullOrEmpty(creatorUserId))
            {
                return BadRequest("User ID claim is missing");
            }
            
            var createdPerformances = await _performanceService.CreateBatchPerformancesAsync(dto, creatorUserId);
            return CreatedAtAction(nameof(GetPerformancesByShowId), new { showId = dto.ShowId }, createdPerformances);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating batch performances");
            return BadRequest(ex.Message);
        }
    }

    // PUT: api/performances/5
    [HttpPut("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> UpdatePerformance(int id, [FromBody] UpdatePerformanceDto dto)
    {
        try
        {
            var updaterUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Updating performance by id {id}", updaterUserId, id);
            var updatedPerformance = await _performanceService.UpdatePerformanceAsync(id, dto, updaterUserId);
            return Ok(updatedPerformance);
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("not found"))
        {
            return NotFound(ex.Message);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating performance");
            return BadRequest(ex.Message);
        }
    }

    // DELETE: api/performances/5
    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin,Manager")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> DeletePerformance(int id)
    {
        try
        {
            var creatorUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            _logger.LogInformation("User {UserId} - Deleting performance by id {id}", creatorUserId, id);
            await _performanceService.DeletePerformanceAsync(id);
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogError(ex, "Error deleting performance");
            return NotFound(ex.Message);
        }
    }
}