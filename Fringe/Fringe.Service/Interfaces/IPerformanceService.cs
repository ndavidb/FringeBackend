using Fringe.Domain.DTOs.PerformanceDTOs;

namespace Fringe.Service.Interfaces;

public interface IPerformanceService
{
    Task<IEnumerable<PerformanceDto>> GetAllPerformancesAsync();
    Task<IEnumerable<PerformanceDto>> GetPerformancesByShowIdAsync(int showId);
    Task<PerformanceDto?> GetPerformanceByIdAsync(int performanceId);
    Task<PerformanceDto> CreatePerformanceAsync(CreatePerformanceDto dto, string creatorUserId);
    Task<List<PerformanceDto>> CreateBatchPerformancesAsync(BatchCreatePerformanceDto dto, string creatorUserId);
    Task<PerformanceDto> UpdatePerformanceAsync(int performanceId, UpdatePerformanceDto dto, string updaterUserId);
    Task<bool> DeletePerformanceAsync(int performanceId);
}