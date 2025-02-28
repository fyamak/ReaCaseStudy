namespace Infrastructure.Mail.Interface;

public interface IMailService
{
    Task SendMailAsync(string subject, string body, string to);
}
