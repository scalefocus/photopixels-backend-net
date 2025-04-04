using SF.PhotoPixels.Domain.Events;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class StateChangesResponseDetails
{
    public Guid Id { get; set; }

    public long Version { get; set; }

    // ReSharper disable once CollectionNeverQueried.Global
    public Dictionary<string, long> Added { get; set; } = new();

    public Dictionary<string, DateTimeOffset> Trashed { get; set; } = new();

    // ReSharper disable once CollectionNeverQueried.Global
    public HashSet<string> Deleted { get; set; } = new();

    public void Apply(MediaObjectCreated media)
    {
        Trashed.Remove(media.ObjectId);
        Deleted.Remove(media.ObjectId);

        Added.TryAdd(media.ObjectId, media.Timestamp);
    }

    public void Apply(MediaObjectTrashed media)
    {
        Added.Remove(media.ObjectId);
        Deleted.Remove(media.ObjectId);

        Trashed.TryAdd(media.ObjectId, media.trashedAt);
    }

    public void Apply(MediaObjectDeleted media)
    {
        Trashed.Remove(media.ObjectId);
        Added.Remove(media.ObjectId);

        Deleted.Add(media.ObjectId);
    }
}