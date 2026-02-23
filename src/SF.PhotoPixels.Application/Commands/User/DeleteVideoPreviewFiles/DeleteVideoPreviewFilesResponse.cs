using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.User.DeleteVideoPreviewFiles;

[GenerateOneOf]
public partial class DeleteVideoPreviewFilesResponse : OneOfBase<Success, NotFound, Forbidden>
{
    public bool IsSuccess { get; set; } = true;
}
