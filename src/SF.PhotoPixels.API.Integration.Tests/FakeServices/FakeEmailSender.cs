using Microsoft.AspNetCore.Identity.UI.Services;

namespace SF.PhotoPixels.API.Integration.Tests.FakeServices;

internal class FakeEmailSender : IEmailSender
{
    public Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        return Task.CompletedTask;
    }
}
