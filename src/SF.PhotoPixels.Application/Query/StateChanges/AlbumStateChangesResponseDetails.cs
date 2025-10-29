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
        Added[objectsAdded.ObjectId] = objectsAdded.AddedAt.ToUnixTimeMilliseconds();
        Removed.Remove(objectsAdded.ObjectId);
    }

    public void Apply(ObjectToAlbumDeleted objectsRemoved)
    {
        if (Added.Remove(objectsRemoved.ObjectId))
        {
            // the photo has been added and removed in the current time interval
            // so just remove it from added and don't add unnecessary information in removed
            return;
        }

        Removed.Add(objectsRemoved.ObjectId);
    }
}
