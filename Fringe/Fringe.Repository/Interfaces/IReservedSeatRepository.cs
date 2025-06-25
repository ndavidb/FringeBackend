namespace Fringe.Repository.Interfaces;

public interface IReservedSeatRepository
{
    Task<ReservedSeat> CreateReservedSeatAsync(ReservedSeat reservedSeat);
    Task<IEnumerable<ReservedSeat>> CreateReservedSeatsAsync(IEnumerable<ReservedSeat> reservedSeats);
    Task<ReservedSeat> UpdateReservedSeatAsync(ReservedSeat reservedSeat);
    
}