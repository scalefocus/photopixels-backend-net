using Mediator;
using OneOf.Types;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.User.SetPreviewConversion;

public class SetPreviewConversionRequest : IRequest<OneOf<Success, ValidationError>>
{
    public required bool PreviewConversion { get; set; }
}
