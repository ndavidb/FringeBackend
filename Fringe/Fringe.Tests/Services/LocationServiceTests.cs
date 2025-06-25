using Fringe.Domain.DTOs.LocationDTOs;
using Fringe.Repository.Interfaces;
using Fringe.Service;

namespace Fringe.Tests.Services;

[TestFixture]
public class LocationServiceTests
{
    
    private Mock<ILocationRepository> _mockLocationRepository;
    private LocationService _locationService;

    [SetUp]
    public void Setup()
    {
        _mockLocationRepository = new Mock<ILocationRepository>();
        _locationService = new LocationService(_mockLocationRepository.Object);
    }

    [Test]
    public async Task GetAllLocationsAsync_ReturnsMappedLocations()
    {
        // Arrange
        var locations = new List<Location>
        {
            new() { LocationId = 1, LocationName = "Location 1" },
            new() { LocationId = 2, LocationName = "Location 2" }
        };

        _mockLocationRepository.Setup(r => r.GetAllAsync())
            .ReturnsAsync(locations);

        // Act
        var result = (await _locationService.GetAllLocationsAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].LocationName.Should().Be("Location 1");
        result[1].LocationName.Should().Be("Location 2");
    }

    [Test]
    public async Task GetLocationByIdAsync_WithValidId_ReturnsLocation()
    {
        // Arrange
        var location = new Location { LocationId = 1, LocationName = "Test Location" };

        _mockLocationRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(location);

        // Act
        var result = await _locationService.GetLocationByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.LocationId.Should().Be(1);
        result.LocationName.Should().Be("Test Location");
    }

    [Test]
    public async Task GetLocationByIdAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        _mockLocationRepository.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Location)null!);

        // Act
        var result = await _locationService.GetLocationByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task CreateLocationAsync_SavesLocationAndReturnsDto()
    {
        // Arrange
        var createLocationDto = new CreateLocationDto
        {
            LocationName = "New Location",
            Address = "123 Street",
            Suburb = "Suburbia",
            PostalCode = "12345",
            State = "State",
            Country = "Country",
            Latitude = 10.123,
            Longitude = 20.456,
            ParkingAvailable = true
        };

        var createdLocation = new Location
        {
            LocationId = 1,
            LocationName = "New Location",
            Address = "123 Street",
            Suburb = "Suburbia",
            PostalCode = "12345",
            State = "State",
            Country = "Country",
            Latitude = 10.123,
            Longitude = 20.456,
            ParkingAvailable = true
        };

        _mockLocationRepository.Setup(r => r.AddAsync(It.IsAny<Location>()))
            .ReturnsAsync(createdLocation);

        var creatorUserId = Guid.NewGuid().ToString();

        // Act
        var result = await _locationService.CreateLocationAsync(createLocationDto, creatorUserId);

        // Assert
        result.Should().NotBeNull();
        result.LocationId.Should().Be(1);
        result.LocationName.Should().Be("New Location");

        _mockLocationRepository.Verify(r => r.AddAsync(It.Is<Location>(l =>
            l.LocationName == createLocationDto.LocationName &&
            l.Address == createLocationDto.Address &&
            l.Latitude == createLocationDto.Latitude &&
            l.Longitude == createLocationDto.Longitude
        )), Times.Once);
    }

    [Test]
    public async Task UpdateLocationAsync_WithValidId_UpdatesLocationAndReturnsDto()
    {
        // Arrange
        var locationId = 1;
        var existingLocation = new Location
        {
            LocationId = locationId,
            LocationName = "Old Location",
            Address = "Old Address"
        };

        var updateDto = new CreateLocationDto
        {
            LocationName = "Updated Location",
            Address = "Updated Address",
            Suburb = "Updated Suburb",
            PostalCode = "99999",
            State = "Updated State",
            Country = "Updated Country",
            Latitude = 11.111,
            Longitude = 22.222,
            ParkingAvailable = false
        };

        _mockLocationRepository.Setup(r => r.GetByIdAsync(locationId))
            .ReturnsAsync(existingLocation);

        var updaterUserId = Guid.NewGuid().ToString();

        // Act
        var result = await _locationService.UpdateLocationAsync(locationId, updateDto, updaterUserId);

        // Assert
        result.Should().NotBeNull();
        result.LocationId.Should().Be(locationId);
        result.LocationName.Should().Be("Updated Location");
        result.Address.Should().Be("Updated Address");

        _mockLocationRepository.Verify(r => r.UpdateAsync(It.Is<Location>(l =>
            l.LocationId == locationId &&
            l.LocationName == updateDto.LocationName &&
            l.Address == updateDto.Address
        )), Times.Once);
    }

    [Test]
    public async Task UpdateLocationAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        _mockLocationRepository.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Location)null);

        var updateDto = new CreateLocationDto
        {
            LocationName = "Updated Location",
            Address = "Updated Address"
        };

        var updaterUserId = Guid.NewGuid().ToString();

        // Act & Assert
        await _locationService.Invoking(s => s.UpdateLocationAsync(99, updateDto, updaterUserId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Location with ID 99 not found");
    }

    [Test]
    public async Task DeleteLocationAsync_WithValidId_DeletesLocation()
    {
        // Arrange
        var locationId = 1;
        var location = new Location { LocationId = locationId };

        _mockLocationRepository.Setup(r => r.LocationExistsAsync(locationId))
            .ReturnsAsync(true);

        _mockLocationRepository.Setup(r => r.DeleteAsync(locationId))
            .ReturnsAsync(true);

        // Act
        await _locationService.DeleteLocationAsync(locationId);

        // Assert
        _mockLocationRepository.Verify(r => r.DeleteAsync(locationId), Times.Once);
    }

    [Test]
    public async Task DeleteLocationAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        _mockLocationRepository.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Location)null);

        // Act & Assert
        await _locationService.Invoking(s => s.DeleteLocationAsync(99))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Location with ID 99 not found");
    }
    
}