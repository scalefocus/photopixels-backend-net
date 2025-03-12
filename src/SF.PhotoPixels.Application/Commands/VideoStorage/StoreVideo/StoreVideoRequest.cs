using Mediator;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.VideoStorage.StoreVideo;

public class StoreVideoRequest : StoreMediaRequest, IRequest<OneOf<IMediaResponse, Duplicate, ValidationError>>
{
}