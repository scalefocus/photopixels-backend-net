using OneOf;

namespace SF.PhotoPixels.Application.Commands.Album.GetAlbum;

public class GetAlbumsResponse
{
    public required IEnumerable<string> Albums { get; set; }
}

[GenerateOneOf]
public partial class GetAlbumsResponses : OneOfBase<GetAlbumsResponse, ValidationError>
{
}
