namespace Fringe.Repository.Interfaces;

public interface ITicketRepository
{
    Task<IEnumerable<Ticket>> GetAllTicketsAsync();
    Task<Ticket?> GetTicketByIdAsync(int ticketId);
    Task<IEnumerable<Ticket>> GetTicketsByUserIdAsync(Guid userId);
    Task<IEnumerable<Ticket>> GetTicketsByBookingReferenceAsync(string bookingReference);
    Task<IEnumerable<Ticket>> GetGroupedTicketsByBookingReferenceAsync();
    Task<IEnumerable<Ticket>> GetTicketsByPerformanceIdAsync(int performanceId);
    Task<Ticket> CreateTicketAsync(Ticket ticket);
    Task<Ticket> UpdateTicketAsync(Ticket ticket);
    Task<IEnumerable<Ticket>> UpdateTicketsByBookingRefAsync(List<Ticket> tickets);
    Task<bool> DeleteTicketsByBookingRefAsync(string bookingRef);
    Task<bool> DeleteTicketAsync(int ticketId);
    Task<bool> TicketExistsAsync(int ticketId);
}