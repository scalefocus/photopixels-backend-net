using SF.PhotoPixels.Domain.Events;

namespace SF.PhotoPixels.Application.Query.StateChanges;

public class StateChangesResponseDetails
{
    public Guid Id { get; set; }
    public long Version { get; set; }
    public Album Albums { get; set; } = new();

    // ReSharper disable once CollectionNeverQueried.Global
    public Dictionary<string, long> Added { get; set; } = new();
    public Dictionary<string, DateTimeOffset> AddedTime { get; set; } = new();
    public Dictionary<string, DateTimeOffset> Trashed { get; set; } = new();

    // ReSharper disable once CollectionNeverQueried.Global
    public HashSet<string> Deleted { get; set; } = new();

    public void Apply(MediaObjectCreated media)
    {
        Trashed.Remove(media.ObjectId);
        Deleted.Remove(media.ObjectId);

        Added.TryAdd(media.ObjectId, media.Timestamp);
        AddedTime.TryAdd(media.ObjectId, DateTimeOffset.FromUnixTimeMilliseconds(media.Timestamp));
    }

    public void Apply(AlbumCreated album)
    {
        //Add the album
        Albums.Added.TryAdd(album.AlbumId.ToString(), album.Timestamp);

        //Remove its id if already exist due to previous apply invocations
        Albums.Updated.Remove(album.AlbumId.ToString());
        Albums.Deleted.Remove(album.AlbumId.ToString());
    }

    public void Apply(AlbumUpdated ev)
    {
        //Set the album as updated
        Albums.Updated[ev.AlbumId.ToString()] = ev.UpdatedAt;
    }

    public void Apply(MediaObjectTrashed media)
    {
        Added.Remove(media.ObjectId);
        AddedTime.Remove(media.ObjectId);
        Deleted.Remove(media.ObjectId);

        Trashed.TryAdd(media.ObjectId, media.trashedAt);
    }

    public void Apply(MediaObjectDeleted media)
    {
        Trashed.Remove(media.ObjectId);
        Added.Remove(media.ObjectId);
        AddedTime.Remove(media.ObjectId);

        Deleted.Add(media.ObjectId);
    }

    public void Apply(AlbumDeleted ev)
    {
        //Remove the album id from the previous collection if exist
        Albums.Added.Remove(ev.AlbumId.ToString());
        Albums.Updated.Remove(ev.AlbumId.ToString());

        //Add it as deleted
        Albums.Deleted.Add(ev.AlbumId.ToString());
    }
}

public class Album
{
    public Dictionary<string, DateTimeOffset> Added { get; set; } = new();
    public Dictionary<string, DateTimeOffset> Updated { get; set; } = new();
    public HashSet<string> Deleted { get; set; } = new();
}
