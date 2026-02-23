using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.User.GetVideoPreviewFilesSize;

public class GetVideoPreviewFilesSizeRequest : IQuery<OneOf<GetVideoPreviewFilesSizeResponse, NotFound>>
{
}