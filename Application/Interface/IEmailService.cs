namespace ASP.NET_Hands_on.Application.Interface
{
    public interface IEmailService
    {
        bool CheckValidEmail(string email);
        Task SendNotificationAsync(string toEmail, string subject, string body);
    }
}
