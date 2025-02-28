using Infrastructure.Mail.Interface;
using MailKit.Net.Smtp;
using MimeKit;
using Microsoft.Extensions.Options;
using Shared.Models;
using Shared.Models.Configuration;

namespace Infrastructure.Mail;

public class MailService : IMailService
{
    private readonly string _host;
    private readonly int    _port;

    private readonly string _senderMail;
    private readonly string _senderMailPassword;

    private readonly SmtpClient _smtpClient;

    public MailService(IOptions<ConfigurationOptions.SmtpOptions> smtpOptions)
    {
        _host = smtpOptions.Value.Host;
        _port = smtpOptions.Value.Port;

        _senderMail         = smtpOptions.Value.Email;
        _senderMailPassword = smtpOptions.Value.Password;

        _smtpClient = new SmtpClient();
    }

    public async Task SendMailAsync(string subject, string body, string to)
    {
        try
        {
            var email = new MimeMessage();

            email.From.Add(MailboxAddress.Parse(_senderMail));

            email.To.AddRange(InternetAddressList.Parse(to));

            email.Subject = subject;

            var builder = new BodyBuilder() { HtmlBody = body };

            email.Body = builder.ToMessageBody();

            await _smtpClient.ConnectAsync(host: _host, port: _port, useSsl: true);

            await _smtpClient.AuthenticateAsync(userName: _senderMail, password: _senderMailPassword);

            await _smtpClient.SendAsync(email);
        }
        finally
        {
            if (_smtpClient.IsConnected)
            {
                await _smtpClient.DisconnectAsync(true);
            }
        }
    }
}
