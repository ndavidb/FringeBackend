using Fringe.Domain.DTOs.Booking;


namespace Fringe.Service.Interfaces
{
    public interface IBookingService
    {
        Task<ServiceResult> CreateBookingAsync(CreateBookingDto createBookingDto);
        Task<TicketDetailsDto> GetBookingByTicketIdAsync(int ticketId);
        Task<List<TicketDetailsDto>> GetBookingsByUserIdAsync(int userId);
        Task<ServiceResult> CancelTicketAsync(int ticketId);

        Task<BookingConfirmationDto> GetBookingConfirmationAsync(string bookingReference);
    }
}
