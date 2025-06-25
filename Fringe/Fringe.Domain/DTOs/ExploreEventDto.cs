using System;

namespace Fringe.Domain.DTOs
{
    public class ExploreEventDto
    {
        public string Title { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public string Location { get; set; }
        public DateTime Date { get; set; }
        public TimeSpan StartTime { get; set; }
        public decimal Price { get; set; }
    }
}
