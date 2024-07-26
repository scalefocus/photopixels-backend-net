using Mediator;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.User.ResetPassword;

public class AdminResetPasswordRequest : IRequest<OneOf<Success, ValidationError, BusinessLogicError>>
{
    public required string Password { get; set; }

    public required string Email { get; set; }
}
