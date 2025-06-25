

using System.Collections.Generic;
using System.Threading.Tasks;
using Fringe.Domain.DTOs;
using Fringe.Repository.Interfaces;
using Fringe.Service.Interfaces;

namespace Fringe.Service
{
    public class ReportService : IReportService
    {
        private readonly IReportRepository _reportRepository;

        public ReportService(IReportRepository reportRepository)
        {
            _reportRepository = reportRepository;
        }

        public async Task<List<ShowSalesReportDto>> GetShowSalesReportAsync(DateTime? startDate, DateTime? endDate)
        {
            return await _reportRepository.GetShowSalesReportAsync(startDate, endDate);
        }
    }
}
