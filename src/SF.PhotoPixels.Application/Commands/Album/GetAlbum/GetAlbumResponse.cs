using OneOf;
using SF.PhotoPixels.Application.Query.PhotoStorage.GetObjects;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Commands.Album.GetAlbum;

public class GetAlbumResponse
{
    public required string LastId { get; set; }
    public required List<PropertiesResponse> Properties { get; set; }
}

[GenerateOneOf]
public partial class GetAlbumResponses : OneOfBase<GetAlbumResponse, ValidationError>
{
}

