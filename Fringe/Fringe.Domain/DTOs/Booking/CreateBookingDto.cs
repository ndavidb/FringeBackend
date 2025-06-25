using Fringe.Domain.DTOs.TicketDTO;
using Fringe.Domain.Entities;
using Fringe.Domain.Enum;


namespace Fringe.Domain.DTOs.Booking
{
    public class CreateBookingDto
    {
        public int PerformanceId { get; set; }
        public Guid? UserId { get; set; }  // Keeping existing customer/user reference
        public CustomerInfoDto CustomerInfo { get; set; }  // Additional customer info if needed
        public List<TicketRequestDto> Tickets { get; set; }
        public SeatingType SeatingType { get; set; }  // 0 = General Admission, 1 = Customised Seating
        public List<SeatSelectionDto> SelectedSeats { get; set; }  // Required for Customised Seating
        public string SpecialRequests { get; set; }
        public bool Newsletter { get; set; }
        public decimal TotalAmount { get; set; }
    }

    public class CustomerInfoDto
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public string Country { get; set; }
    }

    public class TicketRequestDto
    {
        public int TicketPriceId { get; set; }
        public int Quantity { get; set; }
        public decimal Price { get; set; }
    }

    public class SeatSelectionDto
    {
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
    }

    public class BookingResponseDto
    {
        public List<TicketDto> TicketIds { get; set; }
        public string BookingReference { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
        public string Message { get; set; }
    }

    public class TicketDetailsDto
    {
        public int TicketId { get; set; }
        public int PerformanceId { get; set; }
        public int VenueId { get; set; }
        public int UserId { get; set; }
        public string QrImageUrl { get; set; }
        public string QrInCode { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsCheckedIn { get; set; }
        public DateTime CreatedById { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool Cancelled { get; set; }
        public List<ReservedSeatDto> ReservedSeats { get; set; }
        public decimal Price { get; set; }
        public string TicketTypeName { get; set; }
    }

    public class ReservedSeatDto
    {
        public int ReservedSeatId { get; set; }
        public int RowNumber { get; set; }
        public int SeatNumber { get; set; }
        public int TicketId { get; set; }
        public int SeatingPlanId { get; set; }
    }

    public class ServiceResult
    {
        public bool Success { get; set; }
        public string Message { get; set; }
        public List<TicketDto> TicketIds { get; set; }
        public string BookingReference { get; set; }
        public string Status { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
