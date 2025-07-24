using OneOf;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.Album.AddObjectPropertiesToAlbum;

public class AddObjectPropertiesToAlbumResponse
{
}

[GenerateOneOf]
public partial class AddObjectPropertiesToAlbumResponses : OneOfBase<AddObjectPropertiesToAlbumResponse, ValidationError>
{
}