namespace SF.PhotoPixels.Infrastructure.Storage
{
    public interface IObjectStorage
    {
        public Task StoreObjectAsync(Guid userId, IStorageItem item, string name, CancellationToken cancellationToken = default);

        public ValueTask<FileStream> LoadObjectAsync(Guid userId, string imageName, CancellationToken cancellationToken = default);

        public ValueTask<FileStream> LoadThumbnailAsync(Guid userId, string imageName, CancellationToken cancellationToken = default);

        Task<long> StoreThumbnailAsync(Guid userId, IStorageItem item, string name, CancellationToken cancellationToken = default);

        public void DeleteObject(Guid userId, string name);

        public void DeleteThumbnail(Guid userId, string name);

        public void DeleteUserFolders(Guid userId);
    }
}