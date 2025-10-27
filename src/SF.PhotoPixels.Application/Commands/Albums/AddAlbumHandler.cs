using Mediator;
using OneOf;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;

namespace SF.PhotoPixels.Application.Commands.Albums;

public class AddAlbumHandler : IRequestHandler<AddAlbumRequest, OneOf<AddAlbumResponse, ValidationError>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly IAlbumRepository _albumRepository;

    public AddAlbumHandler(IExecutionContextAccessor executionContextAccessor, IAlbumRepository albumRepository)
    {
        _executionContextAccessor = executionContextAccessor;
        _albumRepository = albumRepository;
    }

    public async ValueTask<OneOf<AddAlbumResponse, ValidationError>> Handle(AddAlbumRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(request.Name))
        {
            return new ValidationError("IllegalUserInput", "Name cannot be null or empty");
        }

        var evt = new AlbumCreated
        {
            AlbumId = Guid.NewGuid(),
            Name = request.Name,
            IsSystem = false,
            Timestamp = DateTimeOffset.UtcNow,
            UserId = _executionContextAccessor.UserId
        };

        await _albumRepository.AddAlbumEvent(evt.UserId, evt, cancellationToken);

        return new AddAlbumResponse
        {
            Id = evt.AlbumId.ToString(),
            Name = evt.Name,
        };
    }
}
