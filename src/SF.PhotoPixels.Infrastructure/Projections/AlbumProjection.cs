using Marten;
using Marten.Events.Projections;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;

namespace SF.PhotoPixels.Infrastructure.Projections
{
    public class AlbumProjection : EventProjection
    {
        public AlbumProjection()
        {
            Project<AlbumCreated>(CreateAlbum);
            Project<AlbumDeleted>(DeleteAlbum);
            Project<AlbumUpdated>(UpdateAlbum);
            Project<ObjectToAlbumCreated>(AddObjectToAlbum);
            Project<ObjectToAlbumDeleted>(RemoveObjectToAlbum);
        }

        private static void CreateAlbum(AlbumCreated albumCreated, IDocumentOperations documentOperations)
        {
            var album = new Album()
            {
                Id = albumCreated.AlbumId.ToString(),
                Name = albumCreated.Name,
                DateCreated = albumCreated.CreatedAt,
                UserId = albumCreated.UserId,
                IsSystem = albumCreated.IsSystem
            };

            documentOperations.Store(album);
        }

        private static void DeleteAlbum(AlbumDeleted albumDeleted, IDocumentOperations documentOperations)
        {
            var album = documentOperations.Query<Album>()
                .SingleOrDefault(x => x.Id == albumDeleted.AlbumId.ToString());

            if (album is not null)
            {
                documentOperations.HardDelete<Album>(albumDeleted.AlbumId.ToString());
            }
        }

        private static void UpdateAlbum(AlbumUpdated albumUpdated, IDocumentOperations documentOperations)
        {
            var album = documentOperations.Query<Album>()
                .SingleOrDefault(x => x.Id == albumUpdated.AlbumId.ToString());

            if (album is not null)
            {
                album.Name = albumUpdated.Name;
                album.IsSystem = albumUpdated.IsSystem;

                documentOperations.Store(album);
            }
        }
        private static void AddObjectToAlbum(ObjectToAlbumCreated objectToAlbumCreated, IDocumentOperations documentOperations)
        {
            var album = documentOperations.Query<Album>()
                .SingleOrDefault(x => x.Id == objectToAlbumCreated.AlbumId.ToString());

            if (album is not null)
            {
                var albumObject = new AlbumObject(objectToAlbumCreated.AlbumId.ToString(), objectToAlbumCreated.ObjectId);
                documentOperations.Store(albumObject);
            }
        }

        private static void RemoveObjectToAlbum(ObjectToAlbumDeleted objectToAlbumDeleted, IDocumentOperations documentOperations)
        {
            var albumObject = documentOperations.Query<AlbumObject>()
                .SingleOrDefault(x => x.AlbumId == objectToAlbumDeleted.AlbumId.ToString() 
                    && x.ObjectId == objectToAlbumDeleted.ObjectId);

            if (albumObject is not null)
            {
                documentOperations.DeleteWhere<AlbumObject>(x => x.AlbumId == albumObject.AlbumId && x.ObjectId == albumObject.ObjectId);
            }
        }
    }
}
