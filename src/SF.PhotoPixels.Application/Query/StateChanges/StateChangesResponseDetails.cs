using SF.PhotoPixels.Domain.Events;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class StateChangesResponseDetails
{
    public Guid Id { get; set; }

    public long Version { get; set; }

    // ReSharper disable once CollectionNeverQueried.Global
    public Dictionary<string, long> Added { get; set; } = new();

    // ReSharper disable once CollectionNeverQueried.Global
    public HashSet<string> Deleted { get; set; } = new();

    public void Apply(MediaObjectCreated media)
    {
        Deleted.Remove(media.ObjectId);

        Added.TryAdd(media.ObjectId, media.Timestamp);
    }

    public void Apply(MediaObjectDeleted media)
    {
        var result = Added.Remove(media.ObjectId);

        if (!result)
        {
            Deleted.Add(media.ObjectId);
        }
    }
}