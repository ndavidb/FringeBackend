using System.Security.Claims;
using Fringe.API.Controllers;
using Fringe.Domain.DTOs.VenueDTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fringe.Tests.Controllers;

[TestFixture]
public class VenuesControllerTests
{
    
    private Mock<IVenueService> _mockVenueService;
    private Mock<ILogger<VenuesController>> _mockLogger;
    private VenuesController _controller;
    private ClaimsPrincipal _user;

    [SetUp]
    public void Setup()
    {
        _mockVenueService = new Mock<IVenueService>();
        _mockLogger = new Mock<ILogger<VenuesController>>();
        _controller = new VenuesController(_mockVenueService.Object, _mockLogger.Object);

        //Auth user claims
        var userId = Guid.NewGuid().ToString();
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Role, "Admin")
        };

        _user = new ClaimsPrincipal(new ClaimsIdentity(claims, "TestAuthType"));
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = _user }
        };
    }
    
    [Test]
    public async Task GetAllVenues_ReturnsOkResultWithVenues()
    {
        // Arrange
        var venues = new List<VenueDto>
        {
            new() { VenueId = 1, VenueName = "Venue A" },
            new() { VenueId = 2, VenueName = "Venue B" }
        };
        _mockVenueService.Setup(s => s.GetAllVenuesAsync()).ReturnsAsync(venues);

        // Act
        var result = await _controller.GetAllVenues();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(venues);
    }
    
    
    [Test]
    public async Task GetVenueById_WithValidId_ReturnsOkResult()
    {
        // Arrange
        var venueId = 1;
        var venue = new VenueDto { VenueId = venueId, VenueName = "Venue A", Description = "This is a test venue", TypeId = 1};
        _mockVenueService.Setup(s => s.GetVenueByIdAsync(venueId)).ReturnsAsync(venue);

        // Act
        var result = await _controller.GetVenueById(venueId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(venue);
    }
    
    [Test]
    public async Task GetVenueById_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockVenueService.Setup(s => s.GetVenueByIdAsync(It.IsAny<int>())).ReturnsAsync((VenueDto)null);

        // Act
        var result = await _controller.GetVenueById(99);

        // Assert
        result.Should().BeOfType<NotFoundResult>();
    }
    
    
    [Test]
    public async Task CreateVenue_WithValidData_ReturnsCreatedAtAction()
    {
        // Arrange
        var createDto = new CreateVenueDto
        {
            VenueName = "New Venue",
            LocationId = 1,
            TypeId = 1,
            MaxCapacity = 100,
            Description = "Test Venue",
            ContactEmail = "test@venue.com",
            ContactPhone = "123456789",
            VenueUrl = "http://testvenue.com"
        };

        var createdVenue = new VenueDto
        {
            VenueId = 1,
            VenueName = "New Venue",
        };

        _mockVenueService.Setup(s => s.CreateVenueAsync(createDto, It.IsAny<string>()))
            .ReturnsAsync(createdVenue);

        // Act
        var result = await _controller.CreateVenue(createDto);

        // Assert
        result.Should().BeOfType<CreatedAtActionResult>();
        var createdResult = (CreatedAtActionResult)result;
        createdResult.ActionName.Should().Be(nameof(VenuesController.GetVenueById));
        createdResult.RouteValues?["id"].Should().Be(1);
        createdResult.Value.Should().BeEquivalentTo(createdVenue);
    }
    
    [Test]
    public async Task UpdateVenue_WithValidData_ReturnsOk()
    {
        // Arrange
        var venueId = 1;
        var updateDto = new CreateVenueDto
        {
            VenueName = "Updated Venue",
            LocationId = 1,
            TypeId = 1,
            MaxCapacity = 200,
            Description = "Updated Description",
            ContactEmail = "updated@venue.com",
            ContactPhone = "987654321",
            VenueUrl = "http://updatedvenue.com"
        };

        var updatedVenue = new VenueDto
        {
            VenueId = venueId,
            VenueName = "Updated Venue"
        };

        _mockVenueService.Setup(s => s.UpdateVenueAsync(venueId, updateDto, It.IsAny<string>()))
            .ReturnsAsync(updatedVenue);

        // Act
        var result = await _controller.UpdateVenue(venueId, updateDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(updatedVenue);
    }

    [Test]
    public async Task UpdateVenue_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        var venueId = 70;
        var updateDto = new CreateVenueDto
        {
            VenueName = "Invalid Venue",
            LocationId = 1,
            TypeId = 1,
            MaxCapacity = 50,
            Description = "Invalid",
            ContactEmail = "invalid@venue.com",
            ContactPhone = "123",
            VenueUrl = "http://invalid.com"
        };

        _mockVenueService.Setup(s => s.UpdateVenueAsync(venueId, updateDto, It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Venue not found"));

        // Act
        var result = await _controller.UpdateVenue(venueId, updateDto);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
    
    [Test]
    public async Task DeleteVenue_WithValidId_ReturnsNoContent()
    {
        // Arrange
        var venueId = 1;
        _mockVenueService.Setup(s => s.DeleteVenueAsync(venueId)).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteVenue(venueId);

        // Assert
        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteVenue_WithInvalidId_ReturnsNotFound()
    {
        // Arrange
        _mockVenueService.Setup(s => s.DeleteVenueAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Venue not found"));

        // Act
        var result = await _controller.DeleteVenue(99);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
    }
}

