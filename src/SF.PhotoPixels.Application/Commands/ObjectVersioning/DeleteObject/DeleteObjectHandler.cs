using Marten;
using Marten.Linq.SoftDeletes;
using Mediator;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.TrashHardDelete;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Infrastructure;
using SF.PhotoPixels.Infrastructure.Repositories;
using SF.PhotoPixels.Infrastructure.Storage;

namespace SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;

public class DeleteObjectHandler : IRequestHandler<DeleteObjectRequest, ObjectVersioningResponse>
{
    private readonly IMediator _mediatr;

    public DeleteObjectHandler(IMediator mediatr)
    {
        _mediatr = mediatr;
    }

    public async ValueTask<ObjectVersioningResponse> Handle(DeleteObjectRequest request, CancellationToken cancellationToken)
    {
        return await _mediatr.Send(new DeletePermanentRequest { ObjectIds = [request.Id] }, cancellationToken);
    }
}