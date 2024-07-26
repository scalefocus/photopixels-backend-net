using OneOf;

namespace SF.PhotoPixels.Application.Commands.Tus.CreateUpload
{

    public class CreateUploadResponse
    {
    }

    [GenerateOneOf]
    public partial class CreateUploadResponses : OneOfBase<CreateUploadResponse, ValidationError>
    {

    }
}
