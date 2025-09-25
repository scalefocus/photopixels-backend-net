using System.ComponentModel.DataAnnotations;
using Mediator;
using Microsoft.AspNetCore.Mvc;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.Albums;

public class DeleteAlbumRequest : IRequest<OneOf<Success, ValidationError>>
{
    [Required]
    [FromRoute(Name = "albumId")]
    public required string AlbumId { get; set; }
    
    // Parameterless constructor for serialization/deserialization
    public DeleteAlbumRequest() { }
}