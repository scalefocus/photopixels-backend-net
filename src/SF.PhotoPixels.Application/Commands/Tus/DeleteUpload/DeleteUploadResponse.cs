
using OneOf;

namespace SF.PhotoPixels.Application.Commands.Tus.DeleteUpload;

public class DeleteUploadResponse
{

}

[GenerateOneOf]
public partial class DeleteUploadResponses : OneOfBase<DeleteUploadResponse, ValidationError>
{

}