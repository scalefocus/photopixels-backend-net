using FluentValidation;
using Microsoft.Extensions.Options;
using SF.PhotoPixels.Infrastructure.Options;

namespace SF.PhotoPixels.Application.Commands.User.ForgotPassword;

public class ForgotPasswordValidator : AbstractValidator<ForgotPasswordRequest>
{
    private const string ErrorMessage = "Email provider not configured on this server. Please contact the administrator.";

    public ForgotPasswordValidator(IOptions<SmtpOptions> options)
    {
        RuleFor(_ => options.Value.Host)
            .NotEmpty()
            .WithMessage(ErrorMessage)
            .OverridePropertyName("system");

        RuleFor(_ => options.Value.Username)
            .NotEmpty()
            .WithMessage(ErrorMessage)
            .OverridePropertyName("system");
    }
}