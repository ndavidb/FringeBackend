using FluentAssertions;
using Fringe.Domain.DTOs.ShowDTOs;
using Fringe.Domain.Models;
using Fringe.Repository.Interfaces;
using Fringe.Service;
using Moq;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fringe.Tests.Services;

[TestFixture]
public class ShowServiceTests
{
    private Mock<IShowRepository> _mockShowRepository;
    private ShowService _showService;

    [SetUp]
    public void Setup()
    {
        _mockShowRepository = new Mock<IShowRepository>();
        _showService = new ShowService(_mockShowRepository.Object);
    }

    [Test]
    public async Task GetAllShowsAsync_ReturnsAllShows()
    {
        // Arrange
        var shows = new List<Show>
        {
            new() 
            { 
                ShowId = 1, 
                ShowName = "Show 1", 
                VenueId = 1,
                Venue = new Venue { VenueId = 1, VenueName = "Venue 1" },
                ShowTypeId = 1,
                ShowTypeLookup = new ShowTypeLookup { TypeId = 1, ShowType = "Comedy" },
                AgeRestrictionId = 1,
                AgeRestrictionLookup = new AgeRestrictionLookup { AgeRestrictionId = 1, Code = "G" }
            },
            new() 
            { 
                ShowId = 2, 
                ShowName = "Show 2", 
                VenueId = 2,
                Venue = new Venue { VenueId = 2, VenueName = "Venue 2" },
                ShowTypeId = 2,
                ShowTypeLookup = new ShowTypeLookup { TypeId = 2, ShowType = "Drama" },
                AgeRestrictionId = 2,
                AgeRestrictionLookup = new AgeRestrictionLookup { AgeRestrictionId = 2, Code = "PG" }
            }
        };
        
        _mockShowRepository.Setup(r => r.GetAllShowsAsync())
            .ReturnsAsync(shows);

        // Act
        var result = await _showService.GetAllShowsAsync();

        // Assert
        var showDtos = result.ToList();
        showDtos.Should().HaveCount(2);
        showDtos.ElementAt(0).ShowId.Should().Be(1);
        showDtos.ElementAt(0).ShowName.Should().Be("Show 1");
        showDtos.ElementAt(0).VenueName.Should().Be("Venue 1");
        showDtos.ElementAt(0).ShowType.Should().Be("Comedy");
        showDtos.ElementAt(0).AgeRestrictionCode.Should().Be("G");
        
        showDtos.ElementAt(1).ShowId.Should().Be(2);
        showDtos.ElementAt(1).ShowName.Should().Be("Show 2");
    }

    [Test]
    public async Task GetShowByIdAsync_WithValidId_ReturnsShow()
    {
        // Arrange
        var showId = 1;
        var show = new Show
        {
            ShowId = showId,
            ShowName = "Test Show",
            VenueId = 1,
            Venue = new Venue { VenueId = 1, VenueName = "Test Venue" },
            ShowTypeId = 1,
            ShowTypeLookup = new ShowTypeLookup { TypeId = 1, ShowType = "Comedy" },
            AgeRestrictionId = 1,
            AgeRestrictionLookup = new AgeRestrictionLookup { AgeRestrictionId = 1, Code = "G" },
            Description = "Test Description"
        };
        
        _mockShowRepository.Setup(r => r.GetShowByIdAsync(showId))
            .ReturnsAsync(show);

        // Act
        var result = await _showService.GetShowByIdAsync(showId);

        // Assert
        result.Should().NotBeNull();
        result.ShowId.Should().Be(showId);
        result.ShowName.Should().Be("Test Show");
        result.VenueName.Should().Be("Test Venue");
        result.ShowType.Should().Be("Comedy");
        result.AgeRestrictionCode.Should().Be("G");
        result.Description.Should().Be("Test Description");
    }

    [Test]
    public async Task GetShowByIdAsync_WithInvalidId_ReturnsNull()
    {
        // Arrange
        var showId = 99;
        _mockShowRepository.Setup(r => r.GetShowByIdAsync(showId))!
            .ReturnsAsync((Show)null!);

        // Act
        var result = await _showService.GetShowByIdAsync(showId);

        // Assert
        result.Should().BeNull();
    }

    [Test]
    public async Task CreateShowAsync_SavesToRepositoryAndReturnsDto()
    {
        // Arrange
        var createShowDto = new CreateShowDto
        {
            ShowName = "New Show",
            VenueId = 1,
            ShowTypeId = 1,
            Description = "New show description",
            AgeRestrictionId = 1,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(15)
        };
        
        var creatorUserId = Guid.NewGuid().ToString();
        
        var createdShow = new Show
        {
            ShowId = 1,
            ShowName = "New Show",
            VenueId = 1,
            ShowTypeId = 1,
            Description = "New show description",
            AgeRestrictionId = 1,
            StartDate = createShowDto.StartDate,
            EndDate = createShowDto.EndDate,
            Active = true
        };
        
        var showWithRelations = new Show
        {
            ShowId = 1,
            ShowName = "New Show",
            VenueId = 1,
            Venue = new Venue { VenueId = 1, VenueName = "Test Venue" },
            ShowTypeId = 1,
            ShowTypeLookup = new ShowTypeLookup { TypeId = 1, ShowType = "Comedy" },
            AgeRestrictionId = 1,
            AgeRestrictionLookup = new AgeRestrictionLookup { AgeRestrictionId = 1, Code = "G" },
            Description = "New show description"
        };
        
        _mockShowRepository.Setup(r => r.CreateShowAsync(It.IsAny<Show>()))
            .ReturnsAsync(createdShow);
            
        _mockShowRepository.Setup(r => r.GetShowByIdAsync(createdShow.ShowId))
            .ReturnsAsync(showWithRelations);

        // Act
        var result = await _showService.CreateShowAsync(createShowDto, creatorUserId);

        // Assert
        result.Should().NotBeNull();
        result.ShowId.Should().Be(1);
        result.ShowName.Should().Be("New Show");
        result.VenueName.Should().Be("Test Venue");
        result.ShowType.Should().Be("Comedy");
        result.AgeRestrictionCode.Should().Be("G");
        result.Description.Should().Be("New show description");
        
        _mockShowRepository.Verify(r => r.CreateShowAsync(It.Is<Show>(s => 
            s.ShowName == createShowDto.ShowName &&
            s.VenueId == createShowDto.VenueId &&
            s.ShowTypeId == createShowDto.ShowTypeId &&
            s.Description == createShowDto.Description &&
            s.AgeRestrictionId == createShowDto.AgeRestrictionId &&
            s.StartDate == createShowDto.StartDate &&
            s.EndDate == createShowDto.EndDate &&
            s.Active == true
        )), Times.Once);
    }

    [Test]
    public async Task UpdateShowAsync_WithValidId_UpdatesRepositoryAndReturnsDto()
    {
        // Arrange
        var showId = 1;
        var updateShowDto = new UpdateShowDto
        {
            ShowName = "Updated Show",
            VenueId = 2,
            ShowTypeId = 2,
            Description = "Updated description",
            AgeRestrictionId = 2,
            StartDate = DateTime.UtcNow.AddDays(10),
            EndDate = DateTime.UtcNow.AddDays(15),
            Active = true
        };
        
        var updaterUserId = Guid.NewGuid().ToString();
        
        var existingShow = new Show
        {
            ShowId = showId,
            ShowName = "Original Show",
            VenueId = 1,
            ShowTypeId = 1,
            Description = "Original description",
            AgeRestrictionId = 1,
            Active = true
        };
        
        var updatedShow = new Show
        {
            ShowId = showId,
            ShowName = "Updated Show",
            VenueId = 2,
            ShowTypeId = 2,
            Description = "Updated description",
            AgeRestrictionId = 2,
            Active = true
        };
        
        var showWithRelations = new Show
        {
            ShowId = showId,
            ShowName = "Updated Show",
            VenueId = 2,
            Venue = new Venue { VenueId = 2, VenueName = "Venue 2" },
            ShowTypeId = 2,
            ShowTypeLookup = new ShowTypeLookup { TypeId = 2, ShowType = "Drama" },
            AgeRestrictionId = 2,
            AgeRestrictionLookup = new AgeRestrictionLookup { AgeRestrictionId = 2, Code = "PG" },
            Description = "Updated description"
        };
        
        _mockShowRepository.Setup(r => r.GetShowByIdAsync(showId))
            .ReturnsAsync(existingShow);
            
        _mockShowRepository.Setup(r => r.UpdateShowAsync(It.IsAny<Show>()))
            .ReturnsAsync(updatedShow);
            
        _mockShowRepository.Setup(r => r.GetShowByIdAsync(updatedShow.ShowId))
            .ReturnsAsync(showWithRelations);

        // Act
        var result = await _showService.UpdateShowAsync(showId, updateShowDto, updaterUserId);

        // Assert
        result.Should().NotBeNull();
        result.ShowId.Should().Be(showId);
        result.ShowName.Should().Be("Updated Show");
        result.VenueName.Should().Be("Venue 2");
        result.ShowType.Should().Be("Drama");
        result.AgeRestrictionCode.Should().Be("PG");
        result.Description.Should().Be("Updated description");
        
        _mockShowRepository.Verify(r => r.UpdateShowAsync(It.Is<Show>(s => 
            s.ShowId == showId &&
            s.ShowName == updateShowDto.ShowName &&
            s.VenueId == updateShowDto.VenueId &&
            s.ShowTypeId == updateShowDto.ShowTypeId &&
            s.Description == updateShowDto.Description &&
            s.AgeRestrictionId == updateShowDto.AgeRestrictionId &&
            s.Active == updateShowDto.Active
        )), Times.Once);
    }

    [Test]
    public async Task UpdateShowAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var showId = 99;
        var updateShowDto = new UpdateShowDto
        {
            ShowName = "Updated Show",
            VenueId = 1,
            ShowTypeId = 1
        };
        
        var updaterUserId = Guid.NewGuid().ToString();
        
        _mockShowRepository.Setup(r => r.GetShowByIdAsync(showId))!
            .ReturnsAsync((Show)null!);

        // Act & Assert
        await _showService.Invoking(s => s.UpdateShowAsync(showId, updateShowDto, updaterUserId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Show with ID {showId} not found");
    }

    [Test]
    public async Task DeleteShowAsync_WithValidId_DeletesShow()
    {
        // Arrange
        var showId = 1;
        
        _mockShowRepository.Setup(r => r.ShowExistsAsync(showId))
            .ReturnsAsync(true);
            
        _mockShowRepository.Setup(r => r.DeleteShowAsync(showId))
            .ReturnsAsync(true);

        // Act
        var result = await _showService.DeleteShowAsync(showId);

        // Assert
        result.Should().BeTrue();
        _mockShowRepository.Verify(r => r.DeleteShowAsync(showId), Times.Once);
    }

    [Test]
    public async Task DeleteShowAsync_WithInvalidId_ThrowsException()
    {
        // Arrange
        var showId = 99;
        
        _mockShowRepository.Setup(r => r.ShowExistsAsync(showId))
            .ReturnsAsync(false);

        // Act & Assert
        await _showService.Invoking(s => s.DeleteShowAsync(showId))
            .Should().ThrowAsync<InvalidOperationException>()
            .WithMessage($"Show with ID {showId} not found");
    }

    [Test]
    public async Task GetAllAgeRestrictionsAsync_ReturnsAllAgeRestrictions()
    {
        // Arrange
        var ageRestrictions = new List<AgeRestrictionLookup>
        {
            new() { AgeRestrictionId = 1, Code = "G", Description = "General" },
            new() { AgeRestrictionId = 2, Code = "PG", Description = "Parental Guidance" }
        };
        
        _mockShowRepository.Setup(r => r.GetAllAgeRestrictionsAsync())
            .ReturnsAsync(ageRestrictions);

        // Act
        var result = await _showService.GetAllAgeRestrictionsAsync();

        // Assert
        var ageRestrictionDtos = result.ToList();
        ageRestrictionDtos.Should().HaveCount(2);
        ageRestrictionDtos.ElementAt(0).AgeRestrictionId.Should().Be(1);
        ageRestrictionDtos.ElementAt(0).Code.Should().Be("G");
        ageRestrictionDtos.ElementAt(0).Description.Should().Be("General");
        
        ageRestrictionDtos.ElementAt(1).AgeRestrictionId.Should().Be(2);
        ageRestrictionDtos.ElementAt(1).Code.Should().Be("PG");
        ageRestrictionDtos.ElementAt(1).Description.Should().Be("Parental Guidance");
    }

    [Test]
    public async Task GetAllShowTypesAsync_ReturnsAllShowTypes()
    {
        // Arrange
        var showTypes = new List<ShowTypeLookup>
        {
            new() { TypeId = 1, ShowType = "Comedy" },
            new() { TypeId = 2, ShowType = "Drama" }
        };
        
        _mockShowRepository.Setup(r => r.GetAllShowTypesAsync())
            .ReturnsAsync(showTypes);

        // Act
        var result = await _showService.GetAllShowTypesAsync();

        // Assert
        var showTypeLookupDtos = result.ToList();
        showTypeLookupDtos.Should().HaveCount(2);
        showTypeLookupDtos.ElementAt(0).TypeId.Should().Be(1);
        showTypeLookupDtos.ElementAt(0).ShowType.Should().Be("Comedy");
        
        showTypeLookupDtos.ElementAt(1).TypeId.Should().Be(2);
        showTypeLookupDtos.ElementAt(1).ShowType.Should().Be("Drama");
    }
}
