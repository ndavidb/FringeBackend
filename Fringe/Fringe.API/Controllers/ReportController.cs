using System.Threading.Tasks;
using Fringe.Service.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Fringe.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        [HttpGet("show-sales")]
        public async Task<IActionResult> GetShowSalesReport([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var report = await _reportService.GetShowSalesReportAsync(startDate, endDate);
            return Ok(report);
        }
    }
}