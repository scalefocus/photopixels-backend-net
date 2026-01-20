using Mediator;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.User.AllowVideoConversion;

public class AllowVideoConversionRequest : IRequest<OneOf<Success, ValidationError>>
{
    public required bool AllowVideoConversion { get; set; }
}
