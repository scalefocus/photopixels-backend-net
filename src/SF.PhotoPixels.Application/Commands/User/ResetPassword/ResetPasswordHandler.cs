using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.User.ResetPassword;

public class ResetPasswordHandler : IRequestHandler<ResetPasswordRequest, OneOf<Success, ValidationError, BusinessLogicError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;

    public ResetPasswordHandler(UserManager<Domain.Entities.User> userManager)
    {
        _userManager = userManager;
    }

    public async ValueTask<OneOf<Success, ValidationError, BusinessLogicError>> Handle(ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);

        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        if (!await _userManager.VerifyUserTokenAsync(user, TokenOptions.DefaultEmailProvider, "ResetPassword", request.Code))
        {
            return new ValidationError("InvalidCode", "The provided code is invalid");
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

        return new Success();
    }
}