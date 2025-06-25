

namespace Fringe.Domain.Models
{
    public class EmailSetting
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public bool EnableSsl { get; set; } = true;
        public string DeliveryMethod { get; set; } = "Network";
    }
}
