using Marten;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure.Repositories;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.UpdateObject
{
    public class UpdateObjectHandler : IRequestHandler<UpdateObjectRequest, ObjectVersioningResponse>
    {
        private readonly IExecutionContextAccessor _executionContextAccessor;
        private readonly IObjectRepository _objectRepository;
        private readonly IDocumentSession _session;

        public UpdateObjectHandler(IExecutionContextAccessor executionContextAccessor, IObjectRepository objectRepository, IDocumentSession session, IObjectStorage objectStorage)
        {
            _executionContextAccessor = executionContextAccessor;
            _objectRepository = objectRepository;
            _session = session;
        }

        public async ValueTask<ObjectVersioningResponse> Handle(UpdateObjectRequest request, CancellationToken cancellationToken)
        {
            var objectMetadata = await _session.Query<ObjectProperties>()
                .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

            if (objectMetadata == null)
            {
                return new NotFound();
            }

            var evt = new MediaObjectUpdated
            {
                ObjectId = objectMetadata.Id,
                Extension = objectMetadata.Extension,
                MimeType = objectMetadata.MimeType,
                Height = objectMetadata.Height,
                Width = objectMetadata.Width,
                Name = objectMetadata.Name,
                DateCreated = objectMetadata.DateCreated,
                AppleCloudId = request.RequestBody.AppleCloudId == null ? objectMetadata.AppleCloudId : request.RequestBody.AppleCloudId,
                AndroidCloudId = request.RequestBody.AndroidCloudId == null ? objectMetadata.AndroidCloudId : request.RequestBody.AndroidCloudId,
                Hash = objectMetadata.Hash,
                UserId = objectMetadata.UserId,
                SizeInBytes = objectMetadata.SizeInBytes,
            };

            var version = await _objectRepository.AddEvent(_executionContextAccessor.UserId, evt, cancellationToken);

            return new VersioningResponse
            {
                Revision = version,
            };
        }
    }
}