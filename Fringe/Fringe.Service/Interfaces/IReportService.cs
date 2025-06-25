using System.Collections.Generic;
using System.Threading.Tasks;
using Fringe.Domain.DTOs;

namespace Fringe.Service.Interfaces
{
    public interface IReportService
    {
        Task<List<ShowSalesReportDto>> GetShowSalesReportAsync(DateTime? startDate, DateTime? endDate);
    }
}