using Fringe.Domain.DTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fringe.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UserQueryController : ControllerBase
{
    private readonly IUserQueryService _userQueryService;
    private readonly ILogger<UserQueryController> _logger;

    public UserQueryController(IUserQueryService userQueryService, ILogger<UserQueryController> logger)
    {
        _userQueryService = userQueryService;
        _logger = logger;
    }
    
    // POST: api/userquery
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> CreateQuery([FromBody] UserQueryDto userQueryDto)
    {
        try
        {
            _logger.LogInformation("Creating user query {email} ", userQueryDto.Email);
            var createQuery = await _userQueryService.CreateUserQueryAsync(userQueryDto);
            return StatusCode(StatusCodes.Status201Created, createQuery);
           
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating query");
            return BadRequest(ex.Message);
        }
    }
}