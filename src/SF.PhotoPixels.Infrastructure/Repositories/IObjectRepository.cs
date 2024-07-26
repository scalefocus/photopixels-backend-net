namespace SF.PhotoPixels.Infrastructure.Repositories;

public interface IObjectRepository
{
    Task<long> AddEvent(Guid streamId, object evt, CancellationToken cancellationToken);
}