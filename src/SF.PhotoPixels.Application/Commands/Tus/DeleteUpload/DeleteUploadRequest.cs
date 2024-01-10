using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.Tus.DeleteUpload;

public class DeleteUploadRequest : IRequest<OneOf<NotFound, Success>>
{
    public string FileId { get; set; }
}
