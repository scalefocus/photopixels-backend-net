namespace SF.PhotoPixels.Application.TrashHardDelete;

public interface ITrashHardDeleteService
{
    /// <summary>
    /// Should be executed each 24 hour period at desired time, specified in the app configuration.
    /// </summary>
    /// <param name="cancellationToken"></param>
    /// <returns></returns>
    Task EmptyTrashBin(CancellationToken cancellationToken);

    /// <summary>
    /// Empties the trash bin for the specified user.
    /// </summary>
    /// <param name="userid"></param>
    /// <returns></returns>
    Task<IEnumerable<string>> EmptyTrashBin(Guid userid);
}
