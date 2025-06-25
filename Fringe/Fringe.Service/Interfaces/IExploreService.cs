// IExploreService.cs
using System.Collections.Generic;
using System.Threading.Tasks;
using Fringe.Domain.DTOs;

namespace Fringe.Service.Interfaces
{
    public interface IExploreService
    {
        Task<List<ExploreEventDto>> GetExploreEventsAsync();
    }
}
