using Mediator;

namespace SF.PhotoPixels.Application.TrashHardDelete;

public class EmptyTrashBinRequest : IRequest<EmptyTrashBinResponse>
{
    public Guid UserId { get; set; } = Guid.Empty;
}