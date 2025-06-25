using FluentAssertions;
using Fringe.Domain.DTOs.VenueDTOs;
using Fringe.Domain.Models;
using Fringe.Repository.Interfaces;
using Fringe.Service;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace Fringe.Tests.Services;

[TestFixture]
public class VenueServiceTests
{
    private Mock<IVenueRepository> _mockVenueRepository;
    private VenueService _venueService;
    private Mock<ILogger<VenueService>> _mockLogger;
    private Mock<IWebHostEnvironment> _mockWebHostEnvironment;

    [SetUp]
    public void Setup()
    {
        _mockVenueRepository = new Mock<IVenueRepository>();
        _mockLogger = new Mock<ILogger<VenueService>>();
        _mockWebHostEnvironment = new Mock<IWebHostEnvironment>();
        _venueService = new VenueService(_mockVenueRepository.Object, _mockWebHostEnvironment.Object, _mockLogger.Object);
    }

    [Test]
    public async Task GetAllVenuesAsync_ReturnsMappedVenues()
    {
        // Arrange
        var venues = new List<Venue>
        {
            new() { VenueId = 1, VenueName = "Venue 1" },
            new() { VenueId = 2, VenueName = "Venue 2" }
        };

        _mockVenueRepository.Setup(r => r.GetAllVenuesAsync())
            .ReturnsAsync(venues);

        // Act
        var result = (await _venueService.GetAllVenuesAsync()).ToList();

        // Assert
        result.Should().HaveCount(2);
        result[0].VenueName.Should().Be("Venue 1");
        result[1].VenueName.Should().Be("Venue 2");
    }

    [Test]
    public async Task GetVenueByIdAsync_WithValidId_ReturnsVenue()
    {
        // Arrange
        var venue = new Venue { VenueId = 1, VenueName = "Test Venue" };

        _mockVenueRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(venue);

        // Act
        var result = await _venueService.GetVenueByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result.VenueId.Should().Be(1);
        result.VenueName.Should().Be("Test Venue");
    }

    [Test]
    public async Task GetVenueByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        _mockVenueRepository.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Venue)null);

        // Act
        var result = await _venueService.GetVenueByIdAsync(99);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task CreateVenueAsync_SavesVenueAndReturnsDto()
    {
        // Arrange
        var createVenueDto = new CreateVenueDto { 
            VenueName = "New Venue",
            Rows = 10,
            SeatsPerRow = 20,
            MaxCapacity = 250
        };

        _mockVenueRepository.Setup(r => r.VenueNameExistsAsync(createVenueDto.VenueName))
            .ReturnsAsync(false);

        _mockVenueRepository.Setup(r => r.AddAsync(It.IsAny<Venue>()))
            .Returns(Task.CompletedTask)
            .Callback<Venue>(venue => venue.VenueId = 1);

        var creatorUserId = Guid.NewGuid().ToString();

        // Act
        var result = await _venueService.CreateVenueAsync(createVenueDto, creatorUserId);

        // Assert
        result.Should().NotBeNull();
        result.VenueId.Should().Be(1);
        result.VenueName.Should().Be("New Venue");
        
        // Verify the venue was added
        _mockVenueRepository.Verify(r => r.AddAsync(It.IsAny<Venue>()), Times.Once);
    }

    [Test]
    public async Task UpdateVenueAsync_WithValidId_UpdatesVenueAndReturnsDto()
    {
        // Arrange
        var venueId = 1;
        var existingVenue = new Venue { 
            VenueId = venueId, 
            VenueName = "Old Venue",
            SeatingPlan = new SeatingPlan { 
                SeatingPlanId = 1, 
                Rows = 5, 
                SeatsPerRow = 10 
            }
        };
        
        var updatedVenueDto = new CreateVenueDto { 
            VenueName = "Updated Venue",
            Rows = 8,
            SeatsPerRow = 12,
            MaxCapacity = 100
        };

        _mockVenueRepository.Setup(r => r.GetByIdAsync(venueId))
            .ReturnsAsync(existingVenue);

        _mockVenueRepository.Setup(r => r.UpdateAsync(existingVenue))
            .Returns(Task.CompletedTask);

        var updaterUserId = Guid.NewGuid().ToString();

        // Act
        var result = await _venueService.UpdateVenueAsync(venueId, updatedVenueDto, updaterUserId);

        // Assert
        result.Should().NotBeNull();
        result.VenueId.Should().Be(venueId);
        result.VenueName.Should().Be("Updated Venue");
        result.Rows.Should().Be(8);
        result.SeatsPerRow.Should().Be(12);
        
        // Verify the venue was updated
        _mockVenueRepository.Verify(r => r.UpdateAsync(It.IsAny<Venue>()), Times.Once);
    }

    [Test]
    public async Task UpdateVenueAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        _mockVenueRepository.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Venue)null);

        var updateVenueDto = new CreateVenueDto { VenueName = "Some Venue" };
        var updaterUserId = Guid.NewGuid().ToString();

        // Act & Assert
        await _venueService.Invoking(s => s.UpdateVenueAsync(99, updateVenueDto, updaterUserId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Venue with ID 99 not found");
    }

    [Test]
    public async Task DeleteVenueAsync_WithValidId_DeletesVenue()
    {
        // Arrange
        var venue = new Venue { VenueId = 1, Active = true };

        _mockVenueRepository.Setup(r => r.GetByIdAsync(1))
            .ReturnsAsync(venue);

        _mockVenueRepository.Setup(r => r.DeleteAsync(1))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _venueService.DeleteVenueAsync(1);

        // Assert
        result.Should().BeTrue();
        
        // Verify the venue was deleted
        _mockVenueRepository.Verify(r => r.DeleteAsync(1), Times.Once);
    }

    [Test]
    public async Task DeleteVenueAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        _mockVenueRepository.Setup(r => r.GetByIdAsync(99))
            .ReturnsAsync((Venue)null);

        // Act & Assert
        await _venueService.Invoking(s => s.DeleteVenueAsync(99))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Venue with ID 99 not found.");
    }
}
