using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Commands.Tus.SendChunk;

public class UploadChunkRequest : IRequest<UploadChunkResponse>
{
    [FromRoute(Name = "fileId")]
    public string FileId { get; set; }
}
