using Mediator;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.User.ChangePassword
{
    public class ChangePasswordRequest : IRequest<OneOf<Success, ValidationError, BusinessLogicError>>
    {
        public required string OldPassword { get; set; }

        public required string NewPassword { get; set; }
    }
}
