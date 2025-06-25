namespace Fringe.Domain.DTOs
{
    public class ShowSalesReportDto
    {
        public int ShowId { get; set; }
        public string ShowName { get; set; } = string.Empty;
        public int TotalTicketsSold { get; set; }
        public decimal TotalRevenue { get; set; }
    }
}
