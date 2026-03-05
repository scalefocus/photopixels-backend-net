using System.Collections.Concurrent;

namespace SF.PhotoPixels.Infrastructure.Stores;

public static class UserCancellationStore
{
    private static ConcurrentDictionary<Guid, DateTimeOffset?> CancelledUsers = new();

    public static bool IsUserCancelled(Guid userId) => 
        CancelledUsers.ContainsKey(userId);

    public static void AddUser(Guid userId, DateTimeOffset? cancellationDate = null) => 
        CancelledUsers.TryAdd(userId, cancellationDate);

    public static void RemoveUser(Guid userId) =>
        CancelledUsers.TryRemove(userId, out _);
}
