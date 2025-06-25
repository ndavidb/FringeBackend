using Fringe.Service.Interfaces;

namespace Fringe.Repository;

public class TicketTypeRepository : ITicketTypeRepository
{
    private readonly FringeDbContext _context;

    public TicketTypeRepository(FringeDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TicketType>> GetAllTicketTypesAsync()
    {
        return await _context.TicketTypes
            .ToListAsync();
    }

    public async Task<TicketType?> GetTicketTypeByIdAsync(int ticketTypeId)
    {
        return await _context.TicketTypes
            .FirstOrDefaultAsync(t => t.TicketTypeId == ticketTypeId);
    }

    public async Task<TicketType> CreateTicketTypeAsync(TicketType ticketType)
    {
        await _context.TicketTypes.AddAsync(ticketType);
        await _context.SaveChangesAsync();
        return ticketType;
    }

    public async Task<TicketType> UpdateTicketTypeAsync(TicketType ticketType)
    {
        _context.TicketTypes.Update(ticketType);
        await _context.SaveChangesAsync();
        return ticketType;
    }

    public async Task<bool> DeleteTicketTypeAsync(int ticketTypeId)
    {
        var ticketType = await _context.TicketTypes.FindAsync(ticketTypeId);
        if (ticketType == null)
            return false;

        // Check if there are associated tickets
        var hasTickets = await _context.TicketTypes.AnyAsync(t => t.TicketPrices.Any(tp => tp.TicketTypeId == ticketTypeId));
        if (hasTickets)
            throw new InvalidOperationException($"Cannot delete ticket type with ID {ticketTypeId} because it has associated to the pricing structure of a ticket.");

        _context.TicketTypes.Remove(ticketType);
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<bool> TicketTypeExistsAsync(int ticketTypeId)
    {
        return await _context.TicketTypes.AnyAsync(t => t.TicketTypeId == ticketTypeId);
    }

    public async Task<bool> TicketTypeNameExistsAsync(string typeName)
    {
        return await _context.TicketTypes
            .Where(t => t.TypeName.ToLower() == typeName.ToLower())
            .AnyAsync();
    }
}