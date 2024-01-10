using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf.Types;
using OneOf;
using SF.PhotoPixels.Application.Core;

namespace SF.PhotoPixels.Application.Commands.User.ChangePassword;

public class ChangePasswordHandler : IRequestHandler<ChangePasswordRequest, OneOf<Success, ValidationError, BusinessLogicError>>
{
    private readonly UserManager<Domain.Entities.User> _userManager;
    private readonly IExecutionContextAccessor _executionContextAccessor;

    public ChangePasswordHandler(UserManager<Domain.Entities.User> userManager, IExecutionContextAccessor executionContextAccessor)
    {
        _userManager = userManager;
        _executionContextAccessor = executionContextAccessor;

    }

    public async ValueTask<OneOf<Success, ValidationError, BusinessLogicError>> Handle(ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if(request.OldPassword.Equals(request.NewPassword))
        {
            return new BusinessLogicError("SamePassword", "New password is the same as old password");
        }

        var user = await _userManager.FindByIdAsync(_executionContextAccessor.UserId.ToString());

        if (user == null)
        {
            return new ValidationError("UserNotFound", "User not found");
        }

        var result = await _userManager.ChangePasswordAsync(user, request.OldPassword, request.NewPassword);

        if (!result.Succeeded)
        {
            var errors = new Dictionary<string, string[]>();
            foreach(var error in result.Errors)
            {
                errors.Add(error.Code, new[] { error.Description} );
            }
            return new BusinessLogicError(errors);
        }

        await _userManager.UpdateSecurityStampAsync(user);

        return new Success();
    }
}