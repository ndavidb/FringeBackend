namespace Fringe.Repository.Interfaces;

public interface ITicketPriceRepository
{
    Task<IEnumerable<TicketPrice>> GetAllTicketPricesAsync();
    Task<TicketPrice?> GetTicketPriceByIdAsync(int ticketPriceId);
    Task<IEnumerable<TicketPrice>> GetTicketPricesByPerformanceIdAsync(int performanceId);
    Task<IEnumerable<TicketPrice>> GetTicketPricesByTicketTypeIdAsync(int ticketTypeId);
    Task<TicketPrice> CreateTicketPriceAsync(TicketPrice ticketPrice);
    Task<TicketPrice> UpdateTicketPriceAsync(TicketPrice ticketPrice);
    Task<bool> DeleteTicketPriceAsync(int ticketPriceId);
    Task<bool> TicketPriceExistsAsync(int ticketPriceId);
    Task<bool> IsAssociatedWithPerformanceAsync(int ticketPriceId);
}