using Fringe.Domain.DTOs.PerformanceDTOs;
using Fringe.Domain.DTOs.TicketPriceDTOs;
using Fringe.Domain.Enum;
using Microsoft.Extensions.Logging;

namespace Fringe.Service;

public class PerformanceService : IPerformanceService
{
    private readonly IPerformanceRepository _performanceRepository;
    private readonly IShowRepository _showRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketPriceRepository _ticketPriceRepository;
    private readonly ITicketTypeRepository _ticketTypeRepository;
    private readonly ILogger<PerformanceService> _logger;

    public PerformanceService(
        IPerformanceRepository performanceRepository,
        IShowRepository showRepository,
        ITicketRepository ticketRepository,
        ITicketPriceRepository ticketPriceRepository,
        ITicketTypeRepository ticketTypeRepository,
        ILogger<PerformanceService> logger)
    {
        _performanceRepository = performanceRepository;
        _showRepository = showRepository;
        _ticketRepository = ticketRepository;
        _ticketPriceRepository = ticketPriceRepository;
        _ticketTypeRepository = ticketTypeRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<PerformanceDto>> GetAllPerformancesAsync()
    {
        _logger.LogInformation("Getting all performances");
        var performances = await _performanceRepository.GetAllPerformancesAsync();
        return performances.Select(MapToDto);
    }

    public async Task<IEnumerable<PerformanceDto>> GetPerformancesByShowIdAsync(int showId)
    {
        _logger.LogInformation("Getting performances for show ID: {ShowId}", showId);
        var performances = await _performanceRepository.GetPerformancesByShowIdAsync(showId);
        return performances.Select(MapToDto);
    }

    public async Task<PerformanceDto?> GetPerformanceByIdAsync(int performanceId)
    {
        _logger.LogInformation("Getting performance with ID: {PerformanceId}", performanceId);
        var performance = await _performanceRepository.GetPerformanceByIdAsync(performanceId);
        return performance == null ? null : MapToDto(performance);
    }

    public async Task<PerformanceDto> CreatePerformanceAsync(CreatePerformanceDto dto, string creatorUserId)
    {
        _logger.LogInformation("Creating new performance for show ID: {ShowId}", dto.ShowId);
        var show = await _showRepository.GetShowByIdAsync(dto.ShowId);
        
        // Validate show exists
        if (!await _showRepository.ShowExistsAsync(dto.ShowId))
            throw new InvalidOperationException($"Show with ID {dto.ShowId} not found");
        
        if (dto.PerformanceDate.Date < show.StartDate || dto.PerformanceDate > show.EndDate)
            throw new InvalidOperationException($"Performance date must be between the show's start date ({show.StartDate:yyyy-MM-dd}) and end date ({show.EndDate:yyyy-MM-dd})");

        // Validate time range
        if (dto.EndTime <= dto.StartTime)
            throw new InvalidOperationException("End time must be after start time");

        // Check for conflicting performances
        if (await _performanceRepository.HasConflictingPerformancesAsync(
                dto.ShowId, dto.PerformanceDate, dto.StartTime, dto.EndTime))
        {
            throw new InvalidOperationException(
                "There is already a performance scheduled at this venue during the specified time");
        }

        // Convert user ID to integer for database storage
        var guidUserId = Guid.Parse(creatorUserId);
        var creatorId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        var performance = new Performance
        {
            ShowId = dto.ShowId,
            PerformanceDate = dto.PerformanceDate,
            StartTime = dto.StartTime,
            EndTime = dto.EndTime,
            SeatingType = dto.SeatingType,
            SoldOut = dto.SoldOut,
            Cancel = dto.Cancel,
            Active = dto.Active,
            CreatedById = creatorId,
            CreatedAt = DateTime.UtcNow
        };

        var createdPerformance = await _performanceRepository.CreatePerformanceAsync(performance);
        
        if (dto.TicketPrices != null && dto.TicketPrices.Any())
        {
            foreach (var ticketPriceDto in dto.TicketPrices)
            {
                var ticketPrice = new TicketPrice
                {
                    PerformanceId = createdPerformance.PerformanceId,
                    TicketTypeId = ticketPriceDto.TicketTypeId,
                    Price = ticketPriceDto.Price
                };

                await _ticketPriceRepository.CreateTicketPriceAsync(ticketPrice);
            }
        }
        else
        {
            // If no ticket prices are provided, get all ticket types and create default prices
            var ticketTypes = await _ticketTypeRepository.GetAllTicketTypesAsync();
            foreach (var ticketType in ticketTypes)
            {
                var ticketPrice = new TicketPrice
                {
                    PerformanceId = createdPerformance.PerformanceId,
                    TicketTypeId = ticketType.TicketTypeId,
                    Price = 0 // Default price, you might want to set a more appropriate default
                };

                await _ticketPriceRepository.CreateTicketPriceAsync(ticketPrice);
            }
        }

        var result = await _performanceRepository.GetPerformanceByIdAsync(createdPerformance.PerformanceId);

        return MapToDto(result!);
    }

    public async Task<List<PerformanceDto>> CreateBatchPerformancesAsync(BatchCreatePerformanceDto dto,
        string creatorUserId)
    {
        _logger.LogInformation("Creating batch of {Count} performances for show ID: {ShowId}",
            dto.PerformanceDates.Count, dto.ShowId);

        // Validate show exists
        if (!await _showRepository.ShowExistsAsync(dto.ShowId))
            throw new InvalidOperationException($"Show with ID {dto.ShowId} not found");

        // Validate time range
        if (dto.EndTime <= dto.StartTime)
            throw new InvalidOperationException("End time must be after start time");

        var guidUserId = Guid.Parse(creatorUserId);
        var creatorId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        var performances = new List<Performance>();
        var results = new List<PerformanceDto>();
        var errors = new List<string>();

        // Create a performance for each date
        foreach (var date in dto.PerformanceDates)
        {
            // Check for conflicting performances
            if (await _performanceRepository.HasConflictingPerformancesAsync(
                    dto.ShowId, date, dto.StartTime, dto.EndTime))
            {
                errors.Add($"Conflict detected for date {date.ToShortDateString()}. Skipping this date.");
                continue;
            }

            performances.Add(new Performance
            {
                ShowId = dto.ShowId,
                PerformanceDate = date,
                StartTime = dto.StartTime,
                EndTime = dto.EndTime,
                SoldOut = false,
                Cancel = false,
                Active = dto.Active,
                CreatedById = creatorId,
                CreatedAt = DateTime.UtcNow
            });
        }

        if (!performances.Any())
        {
            throw new InvalidOperationException(
                "Could not create any performances due to conflicts: " + string.Join("; ", errors));
        }

        // Create all performances in a batch
        var createdPerformances = await _performanceRepository.CreateBatchPerformancesAsync(performances);

        // Get full performance details including Show information
        foreach (var perf in createdPerformances)
        {
            var fullPerf = await _performanceRepository.GetPerformanceByIdAsync(perf.PerformanceId);
            if (fullPerf != null)
            {
                results.Add(MapToDto(fullPerf));
            }
        }

        if (errors.Any())
        {
            _logger.LogWarning("Created {Count} performances with {ErrorCount} errors: {Errors}",
                results.Count, errors.Count, string.Join("; ", errors));
        }

        return results;
    }

    public async Task<PerformanceDto> UpdatePerformanceAsync(int performanceId, UpdatePerformanceDto dto,
        string updaterUserId)
    {
        _logger.LogInformation("Updating performance with ID: {PerformanceId}", performanceId);

        var performance = await _performanceRepository.GetPerformanceByIdAsync(performanceId);
        if (performance == null)
            throw new InvalidOperationException($"Performance with ID {performanceId} not found");

        // Validate time range
        if (dto.EndTime <= dto.StartTime)
            throw new InvalidOperationException("End time must be after start time");

        // Check for conflicting performances (excluding this one)
        if (await _performanceRepository.HasConflictingPerformancesAsync(
                performance.ShowId, dto.PerformanceDate, dto.StartTime, dto.EndTime, performanceId))
        {
            throw new InvalidOperationException(
                "There is already another performance scheduled at this venue during the specified time");
        }

        var guidUserId = Guid.Parse(updaterUserId);
        var updaterId = (int)(guidUserId.GetHashCode() & 0x7FFFFFFF); // Simple conversion for demo

        performance.PerformanceDate = dto.PerformanceDate;
        performance.StartTime = dto.StartTime;
        performance.EndTime = dto.EndTime;
        performance.SoldOut = dto.SoldOut;
        performance.Cancel = dto.Cancel;
        performance.Active = dto.Active;
        performance.UpdatedId = updaterId;
        performance.UpdatedAt = DateTime.UtcNow;

        var updatedPerformance = await _performanceRepository.UpdatePerformanceAsync(performance);

        // Update ticket prices if provided
        if (dto.TicketPrices != null && dto.TicketPrices.Any())
        {
            // Get existing ticket prices
            var existingTicketPrices = await _ticketPriceRepository.GetTicketPricesByPerformanceIdAsync(performanceId);
            var existingTicketPriceDict = existingTicketPrices.ToDictionary(tp => tp.TicketTypeId);

            foreach (var ticketPriceDto in dto.TicketPrices)
            {
                if (existingTicketPriceDict.TryGetValue(ticketPriceDto.TicketTypeId, out var existingTicketPrice))
                {
                    // Update existing ticket price
                    existingTicketPrice.Price = ticketPriceDto.Price;
                    await _ticketPriceRepository.UpdateTicketPriceAsync(existingTicketPrice);
                }
                else
                {
                    // Create new ticket price
                    var newTicketPrice = new TicketPrice
                    {
                        PerformanceId = performanceId,
                        TicketTypeId = ticketPriceDto.TicketTypeId,
                        Price = ticketPriceDto.Price
                    };

                    await _ticketPriceRepository.CreateTicketPriceAsync(newTicketPrice);
                }
            }

            // Optional: Remove ticket prices that weren't included in the update
            var updatedTicketTypeIds = dto.TicketPrices.Select(tp => tp.TicketTypeId).ToHashSet();
            foreach (var existingTicketPrice in existingTicketPrices)
            {
                if (!updatedTicketTypeIds.Contains(existingTicketPrice.TicketTypeId))
                {
                    await _ticketPriceRepository.DeleteTicketPriceAsync(existingTicketPrice.TicketPriceId);
                }
            }
        }

        var result = await _performanceRepository.GetPerformanceByIdAsync(updatedPerformance.PerformanceId);

        return MapToDto(result!);
    }

    public async Task<bool> DeletePerformanceAsync(int performanceId)
    {
        _logger.LogInformation("Deleting performance with ID: {PerformanceId}", performanceId);

        if (!await _performanceRepository.PerformanceExistsAsync(performanceId))
            throw new InvalidOperationException($"Performance with ID {performanceId} not found");


        return await _performanceRepository.DeletePerformanceAsync(performanceId);
    }

    private static PerformanceDto MapToDto(Performance performance)
    {
        // Get the venue capacity from the show's venue
        int venueCapacity = performance.Show?.Venue?.MaxCapacity ?? 0;

        var seatingPlan = performance.Show?.Venue?.SeatingPlan ?? new SeatingPlan();

        // Calculate remaining seats based on seating type
        int remainingSeats = 0;
        if (performance.SeatingType == SeatingType.GeneralAdmission)
        {
            // For GA, it's capacity minus the number of tickets sold
            remainingSeats = venueCapacity - performance.Tickets.Count;
        }
        else
        {
            // For CustomisedSeating, it's capacity minus the number of reserved seats that have a ticket
            remainingSeats = venueCapacity - performance.Tickets
                .SelectMany(t => t.ReservedSeats)
                .Count();
        }


        return new PerformanceDto
        {
            PerformanceId = performance.PerformanceId,
            ShowId = performance.ShowId,
            ShowName = performance.Show?.ShowName ?? string.Empty,
            PerformanceDate = performance.PerformanceDate,
            StartTime = performance.StartTime,
            EndTime = performance.EndTime,
            SoldOut = performance.SoldOut,
            Cancel = performance.Cancel,
            Active = performance.Active,
            SeatingType = performance.SeatingType,
            RemainingSeats = remainingSeats,
            SeatingPlan = new Domain.DTOs.VenueDTOs.SeatingPlanDto
            {
                Rows = seatingPlan.Rows,
                SeatsPerRow = seatingPlan.SeatsPerRow
            },
            ReservedSeats = performance.Tickets
                .SelectMany(t => t.ReservedSeats)
                .Select(rs => new ReservedSeatDto
                {
                    ReservedSeatId = rs.ReservedSeatId,
                    RowNumber = rs.RowNumber,
                    SeatNumber = rs.SeatNumber,
                })
                .ToList(),
            TicketPrices = (performance.TicketPrices ?? throw new InvalidOperationException("Ticket prices not found"))
                .Select(tp => new TicketPriceDto
                {
                    TicketPriceId = tp.TicketPriceId,
                    TicketTypeName = tp.TicketType?.TypeName ?? string.Empty,
                    Price = tp.Price
                })
                .ToList(),
        };
    }
}