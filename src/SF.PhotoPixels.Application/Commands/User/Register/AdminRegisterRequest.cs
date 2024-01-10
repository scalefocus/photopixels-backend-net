using Mediator;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Domain.Enums;

namespace SF.PhotoPixels.Application.Commands.User.Register
{
    public class AdminRegisterRequest : IRequest<OneOf<Success, ValidationError>>
    {
        public required string Name { get; set; }

        public required string Email { get; set; }

        public required string Password { get; set; }

        public required Role Role { get; set; }
    }
}
