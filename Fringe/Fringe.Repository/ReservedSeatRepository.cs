namespace Fringe.Repository;

public class ReservedSeatRepository : IReservedSeatRepository
{
    private readonly FringeDbContext _context;
    
    public ReservedSeatRepository(FringeDbContext context)
    {
        _context = context;
    }
    
    public async Task<ReservedSeat> CreateReservedSeatAsync(ReservedSeat reservedSeat)
    {
        await _context.Set<ReservedSeat>().AddAsync(reservedSeat);
        await _context.SaveChangesAsync();
        return reservedSeat;
    }
    
    public async Task<IEnumerable<ReservedSeat>> CreateReservedSeatsAsync(IEnumerable<ReservedSeat> reservedSeats)
    {
        await _context.Set<ReservedSeat>().AddRangeAsync(reservedSeats);
        await _context.SaveChangesAsync();
        return reservedSeats;
    }
    
    public async Task<ReservedSeat> UpdateReservedSeatAsync(ReservedSeat reservedSeat)
    {
        _context.Set<ReservedSeat>().Update(reservedSeat);
        await _context.SaveChangesAsync();
        return reservedSeat;
    }
}