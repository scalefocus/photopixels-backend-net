using Mediator;

namespace SF.PhotoPixels.Application.Commands.Tus.SendChunk;

public class UploadChunkRequest : IRequest<UploadChunkResponse>
{
    public string FileId { get; set; }
}
