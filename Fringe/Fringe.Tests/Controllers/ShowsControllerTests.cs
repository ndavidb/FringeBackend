using System.Security.Claims;
using Fringe.API.Controllers;
using Fringe.Domain.DTOs.ShowDTOs;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace Fringe.Tests.Controllers;

[TestFixture]
public class ShowsControllerTests
{
        private Mock<IShowService> _mockShowService;
        private Mock<ILogger<ShowsController>> _mockLogger;
        private ShowsController _controller;
        private ClaimsPrincipal _user;

        [SetUp]
        public void Setup()
        {
            _mockShowService = new Mock<IShowService>();
            _mockLogger = new Mock<ILogger<ShowsController>>();
            _controller = new ShowsController(_mockShowService.Object, _mockLogger.Object);
            
            // Setup user claims for authorization tests
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
        public async Task GetAllShows_ReturnsOkResultWithShows()
        {
            // Arrange
            var expectedShows = new List<ShowDto>
            {
                new() { ShowId = 1, ShowName = "Comedy Show", ShowType = "Comedy" },
                new() { ShowId = 2, ShowName = "Drama Show", ShowType = "Drama" }
            };
            
            _mockShowService.Setup(s => s.GetAllShowsAsync())
                .ReturnsAsync(expectedShows);

            // Act
            var result = await _controller.GetAllShows();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeEquivalentTo(expectedShows);
        }

        [Test]
        public async Task GetShowById_WithValidId_ReturnsOkResultWithShow()
        {
            // Arrange
            var showId = 1;
            var expectedShow = new ShowDto 
            { 
                ShowId = showId, 
                ShowName = "Test Show", 
                VenueId = 1,
                VenueName = "Test Venue",
                Description = "Test Description"
            };
            
            _mockShowService.Setup(s => s.GetShowByIdAsync(showId))
                .ReturnsAsync(expectedShow);

            // Act
            var result = await _controller.GetShowById(showId);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeEquivalentTo(expectedShow);
        }

        [Test]
        public async Task GetShowById_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var showId = 99;
            _mockShowService.Setup(s => s.GetShowByIdAsync(showId))!
                .ReturnsAsync((ShowDto)null!);

            // Act
            var result = await _controller.GetShowById(showId);

            // Assert
            result.Should().BeOfType<NotFoundResult>();
        }

        [Test]
        public async Task CreateShow_WithValidData_ReturnsCreatedAtActionResult()
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
            
            var createdShow = new ShowDto
            {
                ShowId = 1,
                ShowName = "New Show",
                VenueId = 1,
                ShowTypeId = 1,
                Description = "New show description"
            };
            
            _mockShowService.Setup(s => s.CreateShowAsync(createShowDto, It.IsAny<string>()))
                .ReturnsAsync(createdShow);

            // Act
            var result = await _controller.CreateShow(createShowDto);

            // Assert
            result.Should().BeOfType<CreatedAtActionResult>();
            var createdAtActionResult = (CreatedAtActionResult)result;
            createdAtActionResult.ActionName.Should().Be(nameof(ShowsController.GetShowById));
            createdAtActionResult.RouteValues?["id"].Should().Be(1);
            createdAtActionResult.Value.Should().BeEquivalentTo(createdShow);
        }

        [Test]
        public async Task UpdateShow_WithValidData_ReturnsOkWithUpdatedShow()
        {
            // Arrange
            var showId = 1;
            var updateShowDto = new UpdateShowDto
            {
                ShowName = "Updated Show",
                VenueId = 1,
                ShowTypeId = 1,
                Description = "Updated description",
                AgeRestrictionId = 1,
                StartDate = DateTime.UtcNow.AddDays(10),
                EndDate = DateTime.UtcNow.AddDays(15),
                Active = true
            };
            
            var updatedShow = new ShowDto
            {
                ShowId = showId,
                ShowName = "Updated Show",
                VenueId = 1,
                ShowTypeId = 1,
                Description = "Updated description"
            };
            
            _mockShowService.Setup(s => s.UpdateShowAsync(showId, updateShowDto, It.IsAny<string>()))
                .ReturnsAsync(updatedShow);

            // Act
            var result = await _controller.UpdateShow(showId, updateShowDto);

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeEquivalentTo(updatedShow);
        }

        [Test]
        public async Task UpdateShow_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var showId = 99;
            var updateShowDto = new UpdateShowDto
            {
                ShowName = "Updated Show",
                VenueId = 1,
                ShowTypeId = 1
            };
            
            _mockShowService.Setup(s => s.UpdateShowAsync(showId, updateShowDto, It.IsAny<string>()))
                .ThrowsAsync(new InvalidOperationException("Show with ID 99 not found"));

            // Act
            var result = await _controller.UpdateShow(showId, updateShowDto);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task DeleteShow_WithValidId_ReturnsNoContent()
        {
            // Arrange
            var showId = 1;
            _mockShowService.Setup(s => s.DeleteShowAsync(showId))
                .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteShow(showId);

            // Assert
            result.Should().BeOfType<NoContentResult>();
        }

        [Test]
        public async Task DeleteShow_WithInvalidId_ReturnsNotFound()
        {
            // Arrange
            var showId = 99;
            _mockShowService.Setup(s => s.DeleteShowAsync(showId))
                .ThrowsAsync(new InvalidOperationException("Show with ID 99 not found"));

            // Act
            var result = await _controller.DeleteShow(showId);

            // Assert
            result.Should().BeOfType<NotFoundObjectResult>();
        }

        [Test]
        public async Task GetAgeRestrictions_ReturnsOkWithAgeRestrictions()
        {
            // Arrange
            var ageRestrictions = new List<AgeRestrictionDto>
            {
                new() { AgeRestrictionId = 1, Code = "G", Description = "General" },
                new() { AgeRestrictionId = 2, Code = "PG", Description = "Parental Guidance" }
            };
            
            _mockShowService.Setup(s => s.GetAllAgeRestrictionsAsync())
                .ReturnsAsync(ageRestrictions);

            // Act
            var result = await _controller.GetAgeRestrictions();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeEquivalentTo(ageRestrictions);
        }

        [Test]
        public async Task GetShowTypes_ReturnsOkWithShowTypes()
        {
            // Arrange
            var showTypes = new List<ShowTypeLookupDto>
            {
                new() { TypeId = 1, ShowType = "Comedy" },
                new() { TypeId = 2, ShowType = "Drama" }
            };
            
            _mockShowService.Setup(s => s.GetAllShowTypesAsync())
                .ReturnsAsync(showTypes);

            // Act
            var result = await _controller.GetShowTypes();

            // Assert
            result.Should().BeOfType<OkObjectResult>();
            var okResult = (OkObjectResult)result;
            okResult.Value.Should().BeEquivalentTo(showTypes);
        }
}
