using Mediator;

namespace SF.PhotoPixels.Application.Commands.Tus.DeleteUpload;

public class DeleteUploadRequest : IRequest<DeleteUploadResponses>
{
    public string FileId { get; set; }
}
