using Fringe.Domain;
using Fringe.Domain.DTOs.Booking;
using Fringe.Domain.DTOs.TicketDTO;
using Fringe.Domain.Enum;
using Fringe.Service.Interfaces;
using Microsoft.Extensions.Logging;


namespace Fringe.Service
{
    public class BookingService : IBookingService
    {
        private readonly ITicketService _ticketService;
        private readonly IPerformanceService _performanceService;
        private readonly IEmailService _emailService;
        private readonly ILogger<BookingService> _logger;
        private static int _mockTicketCounter = 1000; // For generating mock ticket IDs

        public BookingService(
            ITicketService ticketService,
            IPerformanceService performanceService,
            IEmailService emailService,
            ILogger<BookingService> logger)
        {
            _ticketService = ticketService;
            _performanceService = performanceService;
            _emailService = emailService;
            _logger = logger;
        }

        public async Task<ServiceResult> CreateBookingAsync(CreateBookingDto createBookingDto)
        {
            try
            {
                // Generate booking reference
                var bookingReference = $"BK{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
                var createdTicketIds = new List<TicketDto>();

                // Validate performance exists
                var performance = await _performanceService.GetPerformanceByIdAsync(createBookingDto.PerformanceId);
                if (performance == null)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = $"Performance with ID {createBookingDto.PerformanceId} not found"
                    };
                }

                // Validate seating type matches

                if ((int)performance.SeatingType != (int)createBookingDto.SeatingType)
                {
                    return new ServiceResult
                    {
                        Success = false,
                        Message = "Seating type mismatch with performance"
                    };
                }

                // Calculate total tickets needed
                var totalTicketsNeeded = createBookingDto.Tickets.Sum(t => t.Quantity);

                // Validate seat selection for customised seating
                if (createBookingDto.SeatingType == SeatingType.CustomisedSeating)
                {
                    if (createBookingDto.SelectedSeats == null || !createBookingDto.SelectedSeats.Any())
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Seat selection is required for reserved seating"
                        };
                    }

                    if (createBookingDto.SelectedSeats.Count != totalTicketsNeeded)
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = $"Number of selected seats ({createBookingDto.SelectedSeats.Count}) must match total tickets ({totalTicketsNeeded})"
                        };
                    }
                }
                else // General Admission
                {
                    if (createBookingDto.SelectedSeats != null && createBookingDto.SelectedSeats.Any())
                    {
                        return new ServiceResult
                        {
                            Success = false,
                            Message = "Seat selection not allowed for general admission"
                        };
                    }
                }
                // Create userID
                Guid userId = Guid.NewGuid();

                // Create tickets
                int seatIndex = 0;
                foreach (var ticketRequest in createBookingDto.Tickets)
                {
                    for (int i = 0; i < ticketRequest.Quantity; i++)
                    {
                        var createTicketDto = new CreateTicketDto
                        {
                            PerformanceId = createBookingDto.PerformanceId,
                            UserId = userId,
                            Price = ticketRequest.Price,
                            TicketPriceId = ticketRequest.TicketPriceId,
                            Quantity = ticketRequest.Quantity
                        };

                        // Customer mapping
                        createTicketDto.CustomerInfo = createBookingDto.CustomerInfo;

                        // Add seat selection for customised seating
                        if (createBookingDto.SeatingType == SeatingType.CustomisedSeating &&
                            createBookingDto.SelectedSeats != null)
                        {
                            createTicketDto.SelectedSeats = new List<SeatSelectionDto>
                            {
                                new SeatSelectionDto
                                {
                                    RowNumber = createBookingDto.SelectedSeats[seatIndex].RowNumber,
                                    SeatNumber = createBookingDto.SelectedSeats[seatIndex].SeatNumber
                                }
                            };
                            seatIndex++;
                        }

                        // Call Ticket Service
                        var ticketResult = await _ticketService.CreateTicketAsync(createTicketDto, userId.ToString(), bookingReference);
                        createdTicketIds.Add(ticketResult);
                    }
                }

                // Send confirmation email
                await SendBookingConfirmationEmailAsync(bookingReference);

                return new ServiceResult
                {
                    Success = true,
                    BookingReference = bookingReference,
                    TicketIds = createdTicketIds,
                    TotalAmount = createBookingDto.TotalAmount,
                    Status = "Confirmed",
                    Message = "Booking created successfully"
                };
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Validation error during booking creation");
                return new ServiceResult
                {
                    Success = false,
                    Message = ex.Message
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating booking");
                return new ServiceResult
                {
                    Success = false,
                    Message = "An error occurred while creating the booking. Please try again."
                };
            }

        }

        /// <summary>
        /// Sends booking confirmation email to the customer
        /// </summary>
        /// <param name="bookingReference">The booking reference number</param>
        /// <returns>Task representing the async operation</returns>
        private async Task SendBookingConfirmationEmailAsync(string bookingReference)
        {
            try
            {
                _logger.LogInformation($"Attempting to send booking confirmation email for booking: {bookingReference}");

                // Get tickets by booking reference
                var tickets = await _ticketService.GetTicketsByBookingReferenceAsync(bookingReference);

                if (tickets == null || !tickets.Any())
                {
                    _logger.LogWarning($"No tickets found for booking reference: {bookingReference}. Cannot send confirmation email.");
                    return;
                }

                // Get customer email from first ticket (all tickets should have same customer)
                var firstTicket = tickets.First();
                var customerEmail = firstTicket.UserEmail;
                var customerName = firstTicket.UserName;

                if (string.IsNullOrEmpty(customerEmail))
                {
                    _logger.LogWarning($"No customer email found for booking reference: {bookingReference}. Cannot send confirmation email.");
                    return;
                }

                // Validate email format
                if (!IsValidEmail(customerEmail))
                {
                    _logger.LogWarning($"Invalid email format for booking reference: {bookingReference}. Email: {customerEmail}");
                    return;
                }

                // Generate email content
                var subject = $"Booking Confirmation - {firstTicket.ShowName} - Reference: {bookingReference}";
                var htmlMessage = GenerateBookingConfirmationEmailHtml(bookingReference, tickets, customerName);

                // Send email using the email service
                await _emailService.SendEmailAsync(customerEmail, subject, htmlMessage);

                _logger.LogInformation($"Booking confirmation email sent successfully for booking: {bookingReference} to {customerEmail}");
            }
            catch (Exception ex)
            {
                // Log the error but don't fail the booking process
                _logger.LogError(ex, $"Failed to send booking confirmation email for booking: {bookingReference}. Booking was created successfully but customer will not receive email confirmation.");

                // Optionally, you could add this to a retry queue or send a notification to admin
                // await AddToEmailRetryQueue(bookingReference);
            }
        }

        /// <summary>
        /// Generates HTML content for the booking confirmation email
        /// </summary>
        /// <param name="bookingReference">The booking reference number</param>
        /// <param name="tickets">List of tickets for this booking</param>
        /// <param name="customerName">Customer's name</param>
        /// <returns>HTML string for the email body</returns>
        private string GenerateBookingConfirmationEmailHtml(string bookingReference, IEnumerable<TicketDto> tickets, string customerName)
        {
            var firstTicket = tickets.First();
            var totalAmount = tickets.Sum(t => (decimal)(t.Price));
            var ticketCount = tickets.Count();

            // Generate individual ticket rows
            var ticketsHtml = string.Join("", tickets.Select((ticket, index) => $@"
            <div style='border: 1px solid #ddd; border-radius: 8px; padding: 20px; margin: 15px 0; background-color: #fafafa;'>
                <div style='display: flex; justify-content: space-between; align-items: center; margin-bottom: 15px;'>
                    <h3 style='color: #7c3aed; margin: 0;'>Ticket {index + 1}</h3>
                    <span style='background: #7c3aed; color: white; padding: 5px 15px; border-radius: 20px; font-size: 12px; font-weight: bold;'>
                        {(ticket.Cancelled ? "CANCELLED" : "ACTIVE")}
                    </span>
                </div>
                
                <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 15px; margin-bottom: 15px;'>
                    <div>
                        <strong>📅 Date:</strong><br>
                        {DateTime.Parse(ticket.PerformanceDate.ToString()).ToString("dddd, MMMM dd, yyyy")}
                    </div>
                    <div>
                        <strong>🕐 Time:</strong><br>
                        {FormatTime(ticket.StartTime.ToString())} - {FormatTime(ticket.EndTime.ToString())}
                    </div>
                    <div>
                        <strong>📍 Venue:</strong><br>
                        {ticket.VenueName}
                    </div>
                    <div>
                        <strong>🎫 Seat:</strong><br>
                        {(ticket.ReservedSeats?.Any() == true ? $"Row {ticket.ReservedSeats.First().RowNumber}, Seat {ticket.ReservedSeats.First().SeatNumber}" : "General Admission")}
                    </div>
                </div>
                
                <div style='display: flex; justify-content: space-between; align-items: center; border-top: 1px solid #ddd; padding-top: 15px;'>
                    <div>
                        <small style='color: #666;'>Ticket ID: #{ticket.TicketId}</small>
                        {(!string.IsNullOrEmpty(ticket.TicketTypeName?.ToString()) ? $"<br><small style='color: #666;'>Type: {ticket.TicketTypeName}</small>" : "")}
                    </div>
                    <div style='text-align: right;'>
                        <strong style='font-size: 18px; color: #7c3aed;'>${(ticket.Price):F2}</strong>
                    </div>
                </div>
            </div>
        "));

            return $@"
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset='utf-8'>
                <meta name='viewport' content='width=device-width, initial-scale=1.0'>
                <title>Booking Confirmation</title>
            </head>
            <body style='font-family: Arial, sans-serif; line-height: 1.6; color: #333; margin: 0; padding: 0; background-color: #f4f4f4;'>
                <div style='max-width: 600px; margin: 0 auto; background-color: white;'>
        
                    <!-- Header -->
                    <div style='background: linear-gradient(135deg, #7c3aed, #a855f7); padding: 30px 20px; text-align: center; color: white;'>
                        <h1 style='margin: 0; font-size: 28px;'>🎉 Booking Confirmed!</h1>
                        <p style='margin: 10px 0 0 0; font-size: 16px; opacity: 0.9;'>Thank you for your purchase, {customerName ?? "Valued Customer"}!</p>
                    </div>

                    <!-- Booking Summary -->
                    <div style='padding: 30px 20px; background-color: #f8f9fa; border-bottom: 1px solid #eee;'>
                        <div style='display: grid; grid-template-columns: 1fr 1fr; gap: 20px; text-align: center;'>
                            <div>
                                <div style='font-size: 14px; color: #666; text-transform: uppercase; letter-spacing: 1px;'>Booking Reference</div>
                                <div style='font-size: 24px; font-weight: bold; color: #7c3aed; margin-top: 5px;'>{bookingReference}</div>
                            </div>
                            <div>
                                <div style='font-size: 14px; color: #666; text-transform: uppercase; letter-spacing: 1px;'>Total Amount</div>
                                <div style='font-size: 24px; font-weight: bold; color: #7c3aed; margin-top: 5px;'>${totalAmount:F2}</div>
                            </div>
                        </div>
                        <div style='text-align: center; margin-top: 15px; color: #666;'>
                            {ticketCount} ticket{(ticketCount != 1 ? "s" : "")} • Show: {firstTicket.ShowName}
                        </div>
                    </div>

                    <!-- QR Code Section -->
                    <div style='padding: 30px 20px; text-align: center; background-color: white;'>
                        <h2 style='color: #333; margin-bottom: 10px;'>📱 Your Entry QR Code</h2>
                        <p style='color: #666; margin-bottom: 20px;'>Show this code at the venue entrance</p>
                        <div style='background: #f8f9fa; padding: 20px; border-radius: 12px; display: inline-block; border: 2px dashed #7c3aed;'>
                            <img src='{firstTicket.QRImageUrl}' alt='Booking QR Code' style='max-width: 200px; height: auto; display: block;' />
                            <div style='margin-top: 10px; font-family: monospace; font-weight: bold; color: #333; font-size: 14px;'>{firstTicket.QRInCode}</div>
                        </div>
                    </div>

                    <!-- Tickets Section -->
                    <div style='padding: 30px 20px;'>
                        <h2 style='color: #333; margin-bottom: 20px; border-bottom: 2px solid #7c3aed; padding-bottom: 10px;'>🎫 Your Tickets</h2>
                        {ticketsHtml}
                    </div>

                    <!-- Important Information -->
                    <div style='background-color: #e3f2fd; padding: 25px 20px; margin: 0;'>
                        <h3 style='color: #1565c0; margin: 0 0 15px 0; display: flex; align-items: center;'>
                            ℹ️ Important Information
                        </h3>
                        <ul style='margin: 0; padding-left: 20px; color: #333;'>
                            <li style='margin-bottom: 8px;'>Please arrive at least <strong>30 minutes before</strong> the show starts</li>
                            <li style='margin-bottom: 8px;'>Bring your <strong>printed tickets</strong> or show the <strong>QR code</strong> on your mobile device</li>
                            <li style='margin-bottom: 8px;'>One QR code is valid for <strong>all tickets</strong> in this booking</li>
                            <li style='margin-bottom: 8px;'>These tickets are <strong>non-transferable</strong> and valid only for the specified date and time</li>
                            <li style='margin-bottom: 0;'>For any queries, please contact our customer support team</li>
                        </ul>
                    </div>

                    <!-- Footer -->
                    <div style='background-color: #333; color: white; padding: 25px 20px; text-align: center;'>
                        <p style='margin: 0 0 10px 0; font-size: 16px;'>Thank you for choosing our service! 🎭</p>
                        <p style='margin: 0; font-size: 12px; opacity: 0.7;'>
                            This is an automated email. Please do not reply to this message.<br>
                            If you have any questions, please contact our customer support.
                        </p>
                    </div>
                </div>
            </body>
            </html>";
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        /// <param name="email">Email address to validate</param>
        /// <returns>True if email is valid, false otherwise</returns>
        private bool IsValidEmail(string email)
        {
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email && email.Contains(".") && email.Contains("@");
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// Formats time string to readable format
        /// </summary>
        /// <param name="timeString">Time in HH:mm format</param>
        /// <returns>Formatted time string</returns>
        private string FormatTime(string timeString)
        {
            if (TimeSpan.TryParse(timeString, out TimeSpan time))
            {
                return time.ToString(@"h\:mm") + (time.Hours >= 12 ? " PM" : " AM");
            }
            return timeString;
        }

        public async Task<BookingConfirmationDto> GetBookingConfirmationAsync(string bookingReference)
        {
            // Send confirmation email
            // await SendBookingConfirmationEmailAsync(bookingReference);

            // This would normally fetch from database
            // For now, return mock data that matches what the confirmation page expects
            var ticket = await _ticketService.GetTicketsByBookingReferenceAsync(bookingReference);
            var mockConfirmation = new BookingConfirmationDto
            {
                BookingReference = bookingReference,
                Tickets = ticket.ToList()
            };
            return mockConfirmation;
        }


        public async Task<TicketDetailsDto> GetBookingByTicketIdAsync(int ticketId)
        {
            // Mock implementation
            return new TicketDetailsDto
            {
                TicketId = ticketId,
                PerformanceId = 34,
                VenueId = 1,
                UserId = 1,
                QrImageUrl = $"/api/qr/BK-{ticketId}",
                QrInCode = $"BK-{ticketId}",
                StartTime = DateTime.Parse("2025-05-17 19:30:00"),
                EndTime = DateTime.Parse("2025-05-17 22:00:00"),
                IsCheckedIn = false,
                CreatedById = DateTime.UtcNow,
                CreatedDate = DateTime.UtcNow,
                Cancelled = false,
                ReservedSeats = new List<ReservedSeatDto>
                {
                    new ReservedSeatDto
                    {
                        ReservedSeatId = 1,
                        RowNumber = 5,
                        SeatNumber = 12,
                        TicketId = ticketId,
                        SeatingPlanId = 1
                    }
                },
                Price = 95,
                TicketTypeName = "Premium"
            };
        }

        //public async Task<TicketDetailsDto> GetBookingByTicketIdAsync(int ticketId)
        //{
        //    return new TicketDetailsDto();
        //    //var ticket = await _context.Tickets
        //    //    .Include(t => t.Performance)
        //    //        .ThenInclude(p => p.Show)
        //    //    .Include(t => t.Performance.Venue)
        //    //    .Include(t => t.ReservedSeats)
        //    //    .FirstOrDefaultAsync(t => t.TicketId == ticketId);

        //    //if (ticket == null)
        //    //    return null;

        //    //return MapToTicketDetailsDto(ticket);
        //}

        public async Task<List<TicketDetailsDto>> GetBookingsByUserIdAsync(int userId)
        {
            return new List<TicketDetailsDto>();
            //var tickets = await _context.Tickets
            //    .Include(t => t.Performance)
            //        .ThenInclude(p => p.Show)
            //    .Include(t => t.Performance.Venue)
            //    .Include(t => t.ReservedSeats)
            //    .Where(t => t.UserId == userId)
            //    .OrderByDescending(t => t.CreatedDate)
            //    .ToListAsync();

            //return tickets.Select(MapToTicketDetailsDto).ToList();
        }

        public async Task<ServiceResult> CancelTicketAsync(int ticketId)
        {
            //var ticket = await _context.Tickets
            //    .Include(t => t.Performance)
            //    .FirstOrDefaultAsync(t => t.TicketId == ticketId);

            //if (ticket == null)
            //{
            //    return new ServiceResult
            //    {
            //        Success = false,
            //        ErrorMessage = "Ticket not found"
            //    };
            //}

            //if (ticket.Cancelled)
            //{
            //    return new ServiceResult
            //    {
            //        Success = false,
            //        ErrorMessage = "Ticket is already cancelled"
            //    };
            //}

            //// Check if performance date has passed
            //if (ticket.Performance.PerformanceDate < DateTime.UtcNow)
            //{
            //    return new ServiceResult
            //    {
            //        Success = false,
            //        ErrorMessage = "Cannot cancel past performances"
            //    };
            //}

            //ticket.Cancelled = true;
            //ticket.UpdatedDate = DateTime.UtcNow;

            //// Update reserved seats
            //var reservedSeats = await _context.ReservedSeats
            //    .Where(rs => rs.TicketId == ticketId)
            //    .ToListAsync();

            //foreach (var seat in reservedSeats)
            //{
            //    seat.UpdatedDate = DateTime.UtcNow;
            //    // Note: You might want to set a cancelled flag here if your schema supports it
            //}

            //await _context.SaveChangesAsync();

            //// Send cancellation email
            //await _emailService.SendTicketCancellationAsync(ticketId);

            return new ServiceResult { Success = true };
        }

        private string GenerateBookingReference()
        {
            return $"BK{DateTime.UtcNow:yyyyMMdd}{Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper()}";
        }

        private string GenerateQrImageUrl(string bookingReference, int index)
        {
            // Generate QR code URL - this would typically call a QR generation service
            return $"/api/qr/{bookingReference}-{index}";
        }

        private TicketDetailsDto MapToTicketDetailsDto(Ticket ticket)
        {
            return new TicketDetailsDto();
            //return new TicketDetailsDto
            //{
            //    TicketId = ticket.TicketId,
            //    PerformanceId = ticket.PerformanceId,
            //    VenueId = ticket.VenueId,
            //    UserId = ticket.UserId,
            //    QrImageUrl = ticket.QrImageUrl,
            //    QrInCode = ticket.QrInCode,
            //    StartTime = ticket.StartTime,
            //    EndTime = ticket.EndTime,
            //    IsCheckedIn = ticket.IsCheckedIn,
            //    CreatedById = ticket.CreatedById,
            //    CreatedDate = ticket.CreatedDate,
            //    Cancelled = ticket.Cancelled,
            //    ReservedSeats = ticket.ReservedSeats?.Select(rs => new ReservedSeatDto
            //    {
            //        ReservedSeatId = rs.ReservedSeatId,
            //        RowNumber = rs.RowNumber,
            //        SeatNumber = rs.SeatNumber,
            //        TicketId = rs.TicketId,
            //        SeatingPlanId = rs.SeatingPlanId
            //    }).ToList() ?? new List<ReservedSeatDto>(),
            //    // Note: Price and TicketTypeName would need to be fetched from related tables
            //    // depending on your schema
            //};
        }


    }
}
