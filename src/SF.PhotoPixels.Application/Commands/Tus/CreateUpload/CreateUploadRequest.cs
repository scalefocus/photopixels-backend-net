using Mediator;

namespace SF.PhotoPixels.Application.Commands.Tus.CreateUpload
{
    public class CreateUploadRequest : IRequest<CreateUploadResponses>
    {

        public static CreateUploadRequest Instance => LazyInstance.Value;
        private static readonly Lazy<CreateUploadRequest> LazyInstance = new(() => new CreateUploadRequest());
        private CreateUploadRequest()
        {
        }
    }
}
