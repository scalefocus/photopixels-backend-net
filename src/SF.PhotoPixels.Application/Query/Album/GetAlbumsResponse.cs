
namespace SF.PhotoPixels.Application.Query.Album;

public class GetAlbumsResponse
{
    public List<AlbumResponse> Albums { get; set; } = new List<AlbumResponse>();

    public class AlbumResponse
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public bool IsSystem { get; set; }
        public DateTimeOffset DateCreated { get; set; }
    }
}