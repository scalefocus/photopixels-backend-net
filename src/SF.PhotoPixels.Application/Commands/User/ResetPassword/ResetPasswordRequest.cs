using Mediator;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.User.ResetPassword;
public class ResetPasswordRequest : IRequest<OneOf<Success, ValidationError, BusinessLogicError>>
{
    public required string Code { get; set; }

    public required string Password { get; set; }

    public required string Email { get; set; }
}
