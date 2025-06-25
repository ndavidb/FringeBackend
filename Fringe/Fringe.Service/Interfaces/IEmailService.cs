namespace Fringe.Service.Interfaces
{
    public interface IEmailService
    {
        Task SendEmailAsync(string emailTo, string subject, string htmlMessage);
    }
}
