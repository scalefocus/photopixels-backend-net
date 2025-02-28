using Mediator;
using OneOf;

namespace SF.PhotoPixels.Application.Commands.PhotoStorage.StorePhoto;

public class StorePhotoRequest : StoreMediaRequest, IRequest<OneOf<IMediaResponse, Duplicate, ValidationError>>
{
}