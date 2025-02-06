using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using SF.PhotoPixels.Infrastructure.Options;
using Microsoft.Extensions.Logging;
using MailKit.Security;

namespace SF.PhotoPixels.Infrastructure;

public class EmailSender : IEmailSender
{
    private const int DefaultPort = 465;

    private readonly SmtpOptions _smtpOptions;
    private readonly ILogger<EmailSender> _logger;

    public EmailSender(IOptions<SmtpOptions> smtpOptions, ILogger<EmailSender> logger)
    {
        _smtpOptions = smtpOptions.Value;
        _logger = logger;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var from = _smtpOptions.Username;

        var bodyBuilder = new BodyBuilder
        {
            HtmlBody = htmlMessage
        };

        var msg = new MimeMessage();
        msg.From.Add(new MailboxAddress("Photo Pixels", from));
        msg.To.Add(new MailboxAddress("Recipient Name", email));
        msg.Subject = subject;        
        msg.Body = bodyBuilder.ToMessageBody();
        try
        {
            using var client = new SmtpClient();
            client.CheckCertificateRevocation = _smtpOptions.CheckCertificateRevocation;

            SecureSocketOptions secureSocketOption = _smtpOptions.UseSsl ? SecureSocketOptions.SslOnConnect : SecureSocketOptions.StartTlsWhenAvailable;

            await client.ConnectAsync(_smtpOptions.Host, _smtpOptions.Port ?? DefaultPort, secureSocketOption);

            _logger.LogInformation("Connection with mail server made");
            await client.AuthenticateAsync(from, _smtpOptions.Password);
            _logger.LogInformation("Authenticated sender credentials on server");
            var result = await client.SendAsync(msg);
            _logger.LogInformation(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex.Message, ex.StackTrace);           
        }

    }
}