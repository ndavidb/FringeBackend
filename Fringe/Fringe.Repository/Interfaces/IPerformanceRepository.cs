namespace Fringe.Repository.Interfaces;

public interface IPerformanceRepository
{
    Task<IEnumerable<Performance>> GetAllPerformancesAsync();
    Task<IEnumerable<Performance>> GetPerformancesByShowIdAsync(int showId);
    Task<Performance?> GetPerformanceByIdAsync(int performanceId);
    Task<Performance> CreatePerformanceAsync(Performance performance);
    Task<IEnumerable<Performance>> CreateBatchPerformancesAsync(IEnumerable<Performance> performances);
    Task<Performance> UpdatePerformanceAsync(Performance performance);
    Task<bool> DeletePerformanceAsync(int performanceId);
    Task<bool> PerformanceExistsAsync(int performanceId);
    Task<bool> HasConflictingPerformancesAsync(int showId, DateTime performanceDate, TimeSpan startTime, TimeSpan endTime, int? excludePerformanceId = null);
}