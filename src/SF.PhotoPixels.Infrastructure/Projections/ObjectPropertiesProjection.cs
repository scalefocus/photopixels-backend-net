using Marten;
using Marten.Events.Projections;
using Marten.Linq.SoftDeletes;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;

namespace SF.PhotoPixels.Infrastructure.Projections;

public class ObjectPropertiesProjection : EventProjection
{
    public ObjectPropertiesProjection()
    {
        Project<MediaObjectCreated>(CreateObjectProperties);
        Project<MediaObjectDeleted>(DeleteObjectProperties);
        Project<MediaObjectUpdated>(UpdateObjectProperties);
        Project<MediaObjectTrashed>(TrashObjectProperties);
        Project<MediaObjectRemovedFromTrash>(RemovedFromTrashObjectProperties);
    }

    private static void RemovedFromTrashObjectProperties(MediaObjectRemovedFromTrash mediaObjectRemovedFromTrash, IDocumentOperations documentOperations)
    {
        var objectProperties = documentOperations.Query<ObjectProperties>()
            .SingleOrDefault(x => x.Id == mediaObjectRemovedFromTrash.ObjectId);

        if (objectProperties is not null)
        {
            documentOperations.UndoDeleteWhere<ObjectProperties>(x => x.Id == mediaObjectRemovedFromTrash.ObjectId);
        }
    }

    private static void TrashObjectProperties(MediaObjectTrashed mediaObjectTrashed, IDocumentOperations documentOperations)
    {
        var objectProperties = documentOperations.Query<ObjectProperties>()
            .SingleOrDefault(x => x.Id == mediaObjectTrashed.ObjectId);

        if (objectProperties is not null)
        {
            documentOperations.Delete<ObjectProperties>(mediaObjectTrashed.ObjectId);
        }
    }

    private static void CreateObjectProperties(MediaObjectCreated mediaObjectCreated, IDocumentOperations documentOperations)
    {
        var objectProperties = new ObjectProperties(mediaObjectCreated.UserId, mediaObjectCreated.Hash)
        {
            Name = mediaObjectCreated.Name,
            DateCreated = DateTimeOffset.FromUnixTimeMilliseconds(mediaObjectCreated.Timestamp),
            Extension = mediaObjectCreated.Extension,
            MimeType = mediaObjectCreated.MimeType,
            Height = mediaObjectCreated.Height,
            Width = mediaObjectCreated.Width,
            Hash = mediaObjectCreated.Hash,
            OriginalHash = mediaObjectCreated.OriginalHash,
            UserId = mediaObjectCreated.UserId,
            AppleCloudId = mediaObjectCreated.AppleCloudId,
            AndroidCloudId = mediaObjectCreated.AndroidCloudId,
            SizeInBytes = mediaObjectCreated.SizeInBytes,
        };

        var existingObjectProperties = documentOperations.Query<ObjectProperties>()
            .SingleOrDefault(x => x.Id == objectProperties.Id && x.IsDeleted());

        if (existingObjectProperties is not null && existingObjectProperties.Deleted)
        {
            documentOperations.HardDelete(existingObjectProperties);
        }

        documentOperations.Store(objectProperties);
    }

    private static void DeleteObjectProperties(MediaObjectDeleted mediaObjectDeleted, IDocumentOperations documentOperations)
    {
        documentOperations.Delete<ObjectProperties>(mediaObjectDeleted.ObjectId);
    }

    private static void UpdateObjectProperties(MediaObjectUpdated mediaObjectUpdated, IDocumentOperations documentOperations)
    {
        var objectProperties = new ObjectProperties(mediaObjectUpdated.UserId, mediaObjectUpdated.Hash)
        {
            Name = mediaObjectUpdated.Name,
            DateCreated = mediaObjectUpdated.DateCreated,
            Extension = mediaObjectUpdated.Extension,
            MimeType = mediaObjectUpdated.MimeType,
            Height = mediaObjectUpdated.Height,
            Width = mediaObjectUpdated.Width,
            Hash = mediaObjectUpdated.Hash,
            OriginalHash = mediaObjectUpdated.Hash,
            UserId = mediaObjectUpdated.UserId,
            AppleCloudId = mediaObjectUpdated.AppleCloudId,
            AndroidCloudId = mediaObjectUpdated.AndroidCloudId,
            SizeInBytes = mediaObjectUpdated.SizeInBytes,
        };

        documentOperations.Store(objectProperties);
    }
}