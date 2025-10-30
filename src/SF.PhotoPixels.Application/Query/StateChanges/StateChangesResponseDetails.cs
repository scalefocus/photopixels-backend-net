using SF.PhotoPixels.Domain.Events;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class StateChangesResponseDetails
{
    public Guid Id { get; set; }
    public long Version { get; set; }
    public Album Albums { get; set; } = new();

    // ReSharper disable once CollectionNeverQueried.Global
    public Dictionary<string, long> Added { get; set; } = new();
    public Dictionary<string, DateTimeOffset> Trashed { get; set; } = new();

    // ReSharper disable once CollectionNeverQueried.Global
    public HashSet<string> Deleted { get; set; } = new();

    public void Apply(MediaObjectCreated media)
    {
        Trashed.Remove(media.ObjectId);
        Deleted.Remove(media.ObjectId);

        Added[media.ObjectId] = media.Timestamp;
    }

    public void Apply(MediaObjectTrashed media)
    {
        Added.Remove(media.ObjectId);
        Deleted.Remove(media.ObjectId);

        Trashed.TryAdd(media.ObjectId, media.trashedAt);
    }

    public void Apply(MediaObjectRemovedFromTrash media)
    {
        Trashed.Remove(media.ObjectId);
        Deleted.Remove(media.ObjectId);

        Added.TryAdd(media.ObjectId, media.removedFromTrashAt.ToUnixTimeMilliseconds());
    }

    public void Apply(MediaObjectDeleted media)
    {
        Trashed.Remove(media.ObjectId);

        if (Added.Remove(media.ObjectId))
        {
            // if the item has been added and is not removed
            // we don't need its data at all
            return;
        }

        Deleted.Add(media.ObjectId);
    }

    public void Apply(AlbumCreated album)
    {
        Albums.Updated.Remove(album.AlbumId.ToString());
        Albums.Deleted.Remove(album.AlbumId.ToString());

        //keep the last timestamp if the album is added twice
        Albums.Added[album.AlbumId.ToString()] = album.CreatedAt.ToUnixTimeMilliseconds();
    }

    public void Apply(AlbumUpdated ev)
    {
        if(Albums.Added.ContainsKey(ev.AlbumId.ToString()))
        {
            //if the album has been added state during time interval,
            //we don't need to keep in added and and in updated twice.
            //we can keep it as a new album in added and to update its latest information (if needed)
            //Albums.Added[ev.AlbumId.ToString()] = ev.UpdatedAt.ToUnixTimeMilliseconds();
            return;
        }

        Albums.Deleted.Remove(ev.AlbumId.ToString());
        Albums.Updated[ev.AlbumId.ToString()] = ev.UpdatedAt.ToUnixTimeMilliseconds();
    }

    public void Apply(AlbumDeleted ev)
    {
        Albums.Updated.Remove(ev.AlbumId.ToString());

        if (Albums.Added.Remove(ev.AlbumId.ToString()))
        {
            //the album has been added but now it is removed in the current time interval
            //we don't need to keep it in our records, so we remove it from added, updated and don't insert it in deleted
            return;
        }

        //Add it as deleted
        Albums.Deleted.Add(ev.AlbumId.ToString());
    }
}

public class Album
{
    public Dictionary<string, long> Added { get; set; } = new();
    public Dictionary<string, long> Updated { get; set; } = new();
    public HashSet<string> Deleted { get; set; } = new();
}
