using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.User.Register
{
    public class UserRegisterRequest : IRequest<OneOf<Success, ValidationError>>
    {
        public required string Name { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }
    }
}
