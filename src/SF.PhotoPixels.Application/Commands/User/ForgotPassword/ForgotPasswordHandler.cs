using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Infrastructure.Helpers;

namespace SF.PhotoPixels.Application.Commands.User.ForgotPassword;

public class ForgotPasswordHandler : IRequestHandler<ForgotPasswordRequest, OneOf<Success, NotFound>>
{
    private readonly IEmailGenerator _emailGenerator;
    private readonly IEmailSender _emailSender;
    private readonly UserManager<Domain.Entities.User> _userManager;

    public ForgotPasswordHandler(UserManager<Domain.Entities.User> userManager, IEmailSender emailSender, IEmailGenerator emailGenerator)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _emailGenerator = emailGenerator;
    }

    public async ValueTask<OneOf<Success, NotFound>> Handle(ForgotPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user is null)
        {
            return new NotFound();
        }

        var code = await _userManager.GenerateUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "ResetPassword");

        const string subject = "Password reset requested";
        var body = _emailGenerator.CreatePasswordResetEmail(code);

        await _emailSender.SendEmailAsync(user.Email, subject, body);

        return new Success();
    }
}