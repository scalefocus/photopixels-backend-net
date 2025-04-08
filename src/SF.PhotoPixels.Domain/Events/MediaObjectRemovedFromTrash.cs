namespace SF.PhotoPixels.Domain.Events
{

    public record MediaObjectRemovedFromTrash(string ObjectId, DateTimeOffset removedFromTrashAt);
}