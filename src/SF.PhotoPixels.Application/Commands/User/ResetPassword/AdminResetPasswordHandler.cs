using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf.Types;
using OneOf;
using Microsoft.AspNetCore.Identity.UI.Services;
using SF.PhotoPixels.Infrastructure.Helpers;

namespace SF.PhotoPixels.Application.Commands.User.ResetPassword;

public class AdminResetPasswordHandler : IRequestHandler<AdminResetPasswordRequest, OneOf<Success, ValidationError, BusinessLogicError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IEmailGenerator _emailGenerator;
    private readonly IEmailSender _emailSender;
    public AdminResetPasswordHandler(UserManager<Domain.Entities.User> userManager, IEmailSender emailSender, IEmailGenerator emailGenerator)
    {
        _userManager = userManager;
        _emailSender = emailSender;
        _emailGenerator = emailGenerator;
    }

    public async ValueTask<OneOf<Success, ValidationError, BusinessLogicError>> Handle(AdminResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var result = await _userManager.ResetPasswordAsync(user, token, request.Password);

        if (!result.Succeeded)
        {
            var errors = new Dictionary<string, string[]>();
            foreach (var error in result.Errors)
            {
                errors.Add(error.Code, new[] { error.Description });
            }
            return new BusinessLogicError(errors);
        }

        await _userManager.UpdateSecurityStampAsync(user);

        const string subject = "Password Reset Notification";
        var body = _emailGenerator.CreateAdminPasswordResetEmail(user.Name);

        await _emailSender.SendEmailAsync(user.Email, subject, body);

        return new Success();
    }
}
