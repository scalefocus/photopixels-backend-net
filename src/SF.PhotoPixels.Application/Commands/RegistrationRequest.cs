using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands
{
    public class RegistrationRequest : IRequest<OneOf<Success>>
    {
        public bool Value { get; set; }
    }
}