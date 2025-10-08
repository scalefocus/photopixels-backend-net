namespace SF.PhotoPixels.Infrastructure.Repositories;

public interface IAlbumRepository
{
    Task<long> AddAlbumEvent(Guid streamId, object evt, CancellationToken cancellationToken);
}