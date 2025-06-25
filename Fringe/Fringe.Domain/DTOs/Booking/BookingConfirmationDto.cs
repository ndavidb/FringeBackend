using Fringe.Domain.DTOs.TicketDTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fringe.Domain.DTOs.Booking
{
    public class BookingConfirmationDto
    {
        public string BookingReference { get; set; }
        public List<TicketDto> Tickets { get; set; }
    }
}
