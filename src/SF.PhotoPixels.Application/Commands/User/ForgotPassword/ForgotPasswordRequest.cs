using Mediator;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.User.ForgotPassword;
public class ForgotPasswordRequest : IRequest<OneOf<Success, NotFound>>
{
    public required string Email { get; set; }
}
