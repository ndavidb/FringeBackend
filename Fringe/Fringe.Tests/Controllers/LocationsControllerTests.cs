using System.Security.Claims;
using Fringe.API.Controllers;
using Fringe.Domain.DTOs.LocationDTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fringe.Tests.Controllers;

public class LocationsControllerTests
{
    private Mock<ILocationService> _mockLocationService;
    private Mock<ILogger<LocationsController>> _mockLogger;
    private LocationsController _controller;
    private ClaimsPrincipal _user;

    [SetUp]
    public void Setup()
    {
        _mockLocationService = new Mock<ILocationService>();
        _mockLogger = new Mock<ILogger<LocationsController>>();
        _controller = new LocationsController(_mockLocationService.Object, _mockLogger.Object);

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
    public async Task GetAllLocations_ReturnsOkResultWithLocations()
    {
        var expectedLocations = new List<LocationDto>
        {
            new() { LocationId = 1, LocationName = "Adelaide Oval" },
            new() { LocationId = 2, LocationName = "Festival Centre" }
        };

        _mockLocationService.Setup(s => s.GetAllLocationsAsync())
            .ReturnsAsync(expectedLocations);

        var result = await _controller.GetAllLocations();

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedLocations);
    }
    
    [Test]
    public async Task GetLocationById_WithValidId_ReturnsOkResultWithLocation()
    {
        var locationId = 1;
        var expectedLocation = new LocationDto
        {
            LocationId = locationId,
            LocationName = "Adelaide Oval"
        };

        _mockLocationService.Setup(s => s.GetLocationByIdAsync(locationId))
            .ReturnsAsync(expectedLocation);

        var result = await _controller.GetLocationById(locationId);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(expectedLocation);
    }
    
    [Test]
    public async Task CreateLocation_WithValidData_ReturnsCreatedAtActionResult()
    {
        var createLocationDto = new CreateLocationDto
        {
            LocationName = "New Venue",
            Address = "123 Main St"
        };

        var createdLocation = new LocationDto
        {
            LocationId = 1,
            LocationName = "New Venue",
            Address = "123 Main St"
        };

        _mockLocationService.Setup(s => s.CreateLocationAsync(createLocationDto, It.IsAny<string>()))
            .ReturnsAsync(createdLocation);

        var result = await _controller.CreateLocation(createLocationDto);

        result.Should().BeOfType<CreatedAtActionResult>();
        var createdAtResult = (CreatedAtActionResult)result;
        createdAtResult.ActionName.Should().Be(nameof(LocationsController.GetLocationById));
        createdAtResult.RouteValues?["id"].Should().Be(1);
        createdAtResult.Value.Should().BeEquivalentTo(createdLocation);
    }
    
    [Test]
    public async Task UpdateLocation_WithValidData_ReturnsOkWithUpdatedLocation()
    {
        var locationId = 1;
        var updateLocationDto = new CreateLocationDto
        {
            LocationName = "Updated Venue",
            Address = "456 Updated Rd"
        };

        var updatedLocation = new LocationDto
        {
            LocationId = locationId,
            LocationName = "Updated Venue",
            Address = "456 Updated Rd"
        };

        _mockLocationService.Setup(s => s.UpdateLocationAsync(locationId, updateLocationDto, It.IsAny<string>()))
            .ReturnsAsync(updatedLocation);

        var result = await _controller.UpdateShow(locationId, updateLocationDto);

        result.Should().BeOfType<OkObjectResult>();
        var okResult = (OkObjectResult)result;
        okResult.Value.Should().BeEquivalentTo(updatedLocation);
    }
    
    [Test]
    public async Task UpdateLocation_WithInvalidId_ReturnsNotFound()
    {
        var locationId = 100;
        var updateLocationDto = new CreateLocationDto
        {
            LocationName = "Nonexistent Venue"
        };

        _mockLocationService.Setup(s => s.UpdateLocationAsync(locationId, updateLocationDto, It.IsAny<string>()))
            .ThrowsAsync(new InvalidOperationException("Location with ID 99 not found"));

        var result = await _controller.UpdateShow(locationId, updateLocationDto);

        result.Should().BeOfType<NotFoundObjectResult>();
    }
    
    [Test]
    public async Task DeleteLocation_WithValidId_ReturnsNoContent()
    {
        var locationId = 1;

        _mockLocationService.Setup(s => s.DeleteLocationAsync(locationId))
            .Returns(Task.CompletedTask);

        var result = await _controller.Delete(locationId);

        result.Should().BeOfType<NoContentResult>();
    }

    [Test]
    public async Task DeleteLocation_WithInvalidId_ReturnsNotFound()
    {
        var locationId = 100;

        _mockLocationService.Setup(s => s.DeleteLocationAsync(locationId))
            .ThrowsAsync(new InvalidOperationException("Location with ID 99 not found"));

        var result = await _controller.Delete(locationId);

        result.Should().BeOfType<NotFoundObjectResult>();
    }

    
}