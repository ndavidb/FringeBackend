// ExploreService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Fringe.Domain;
using Fringe.Domain.DTOs;
using Microsoft.EntityFrameworkCore;
using Fringe.Service.Interfaces;

namespace Fringe.Service // ✅ Correct namespace for service class
{
    public class ExploreService : IExploreService
    {
        private readonly FringeDbContext _context;

        public ExploreService(FringeDbContext context)
        {
            _context = context;
        }

        public async Task<List<ExploreEventDto>> GetExploreEventsAsync()
        {
            var events = await _context.Performances
                .Include(p => p.Show)
                .ThenInclude(p => p.Venue)
                .Include(p => p.TicketPrices)
                .Select(p => new ExploreEventDto
                {
                    Title = p.Show.ShowName,
                    Description = p.Show.Description,
                    ImageUrl = p.Show.ImagesUrl,
                    Location = p.Show.Venue.Location.LocationName,
                    Date = p.PerformanceDate,
                    StartTime = p.StartTime,
                    Price = p.TicketPrices.Min(tp => tp.Price)
                })
                .ToListAsync();

            return events;
        }
    }
}
