using System.Collections.Generic;
using System.Threading.Tasks;
using Fringe.Domain.DTOs;

namespace Fringe.Repository.Interfaces
{
	public interface IReportRepository
	{
		Task<List<ShowSalesReportDto>> GetShowSalesReportAsync(DateTime? startDate, DateTime? endDate);
	}
}