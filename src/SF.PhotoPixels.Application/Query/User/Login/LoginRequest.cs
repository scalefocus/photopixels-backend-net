using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.User.Login;

public class LoginRequest : IRequest<OneOf<Success, Unauthorized>>
{
    public required string Email { get; set; }
    public required string Password { get; set; }
}