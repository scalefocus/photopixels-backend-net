using Mediator;
using Microsoft.AspNetCore.Mvc;

namespace SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;

public class AddToAlbumResponse
{
    public string ObjectId { get; set; }

    public string AlbumId { get; set; }
}