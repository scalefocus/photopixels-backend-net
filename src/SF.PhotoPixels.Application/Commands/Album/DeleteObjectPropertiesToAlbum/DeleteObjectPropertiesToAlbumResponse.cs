using OneOf;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.Album.DeleteObjectPropertiesToAlbum;

public class DeleteObjectPropertiesToAlbumResponse
{
}

[GenerateOneOf]
public partial class DeleteObjectPropertiesToAlbumResponses : OneOfBase<DeleteObjectPropertiesToAlbumResponse, ValidationError>
{
}