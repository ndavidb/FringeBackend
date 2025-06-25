namespace Fringe.Service.Interfaces;

public interface ITicketTypeRepository
{
    Task<IEnumerable<TicketType>> GetAllTicketTypesAsync();
    Task<TicketType?> GetTicketTypeByIdAsync(int ticketTypeId);
    Task<TicketType> CreateTicketTypeAsync(TicketType ticketType);
    Task<TicketType> UpdateTicketTypeAsync(TicketType ticketType);
    Task<bool> DeleteTicketTypeAsync(int ticketTypeId);
    Task<bool> TicketTypeExistsAsync(int ticketTypeId);
    Task<bool> TicketTypeNameExistsAsync(string typeName);
}