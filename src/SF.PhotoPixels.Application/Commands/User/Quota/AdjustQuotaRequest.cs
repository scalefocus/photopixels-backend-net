using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.User.Quota
{
    public class AdjustQuotaRequest : IRequest<OneOf<AdjustQuotaResponse, NotFound, ValidationError>>
    {
        public required Guid Id { get; set; }

        public required long Quota { get; set; }
    }
}
