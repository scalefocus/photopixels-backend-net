using SF.PhotoPixels.Domain.Events;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class AlbumStateChangesResponseDetails
{
    public Guid Id { get; set; }
    public long Version { get; set; }
    public Dictionary<string, long> Added { get; set; } = new();
    public HashSet<string> Removed { get; set; } = [];

    public void Apply(ObjectToAlbumCreated objectsAdded)
    {
        Added.TryAdd(objectsAdded.ObjectId, objectsAdded.TimeStamp.ToUnixTimeMilliseconds());
        Removed.Remove(objectsAdded.ObjectId);
    }

    public void Apply(ObjectToAlbumDeleted objectsRemoved)
    {
        Removed.Add(objectsRemoved.ObjectId);
        Added.Remove(objectsRemoved.ObjectId);
    }
}
