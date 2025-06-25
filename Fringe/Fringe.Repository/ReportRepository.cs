using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Fringe.Domain;
using Fringe.Domain.DTOs;
using Fringe.Repository.Interfaces;
using Microsoft.EntityFrameworkCore;
namespace Fringe.Repository
{
    public class ReportRepository : IReportRepository
    {
        private readonly FringeDbContext _context;
        public ReportRepository(FringeDbContext context)
        {
            _context = context;
        }
        public async Task<List<ShowSalesReportDto>> GetShowSalesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            var query = _context.Tickets
        .Include(t => t.Performance)
            .ThenInclude(p => p.Show)
        .Where(t => !t.Cancelled) // Exclude cancelled tickets
        .AsQueryable();

            // Optimized date range filtering - both dates must be provided for proper "between" logic
            if (startDate.HasValue && endDate.HasValue)
            {
                var startDateUtc = startDate.Value.Date; // Start of start date
                var endDateUtc = endDate.Value.Date.AddDays(1); // End of end date (exclusive)

                query = query.Where(t => t.CreatedAt >= startDateUtc && t.CreatedAt < endDateUtc);
            }
            else if (startDate.HasValue)
            {
                // Only start date provided - from start date onwards
                var startDateUtc = startDate.Value.Date;
                query = query.Where(t => t.CreatedAt >= startDateUtc);
            }
            else if (endDate.HasValue)
            {
                // Only end date provided - up to end date
                var endDateUtc = endDate.Value.Date.AddDays(1);
                query = query.Where(t => t.CreatedAt < endDateUtc);
            }

            var report = await query
                .GroupBy(t => new {
                    t.Performance.Show.ShowId,
                    t.Performance.Show.ShowName
                })
                .Select(g => new ShowSalesReportDto
                {
                    ShowId = g.Key.ShowId,
                    ShowName = g.Key.ShowName,
                    TotalTicketsSold = g.Sum(t => t.Quantity ?? 1),
                    TotalRevenue = g.Sum(t => (t.Quantity ?? 1) * t.Price)
                })
                .OrderBy(r => r.ShowName)
                .ToListAsync();

            return report;
        }
    }
}