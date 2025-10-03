using Marten;
using Mediator;
using Microsoft.Extensions.Logging;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;

namespace SF.PhotoPixels.Application.Query.Album
{  
    public class GetAlbumInfoHandler : IQueryHandler<GetAlbumInfoRequest, OneOf<GetAlbumInfoResponse, NotFound>>
    {
        private readonly ILogger<GetAlbumInfoHandler> _logger;
        private readonly IDocumentSession _session;
        private readonly IExecutionContextAccessor _executionContextAccessor;

        public GetAlbumInfoHandler(ILogger<GetAlbumInfoHandler> logger, IDocumentSession session, IExecutionContextAccessor executionContextAccessor)
        {
            _logger = logger;
            _session = session;
            _executionContextAccessor = executionContextAccessor;
        }

        public async ValueTask<OneOf<GetAlbumInfoResponse, NotFound>> Handle(GetAlbumInfoRequest request, CancellationToken cancellationToken)
        {
            _logger.LogInformation("Handling {RequestType}", request.GetType().Name);

            var albumInfo = await _session.LoadAsync<Domain.Entities.Album>(request.AlbumId, cancellationToken);
            
            if (albumInfo == null)
            {
                return new NotFound();
            }

            var albumResponses = new GetAlbumInfoResponse();
            albumResponses.AlbumId = albumInfo.Id;
            albumResponses.Name = albumInfo.Name;
            
            return albumResponses;
        }
    }
}
