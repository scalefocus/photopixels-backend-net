using Marten;
using Mediator;
using SF.PhotoPixels.Application.Commands.ObjectVersioning.DeleteObject;
using SF.PhotoPixels.Application.TrashHardDelete;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Repositories;


namespace SF.PhotoPixels.Application.Commands.EmptyTrashBin.StorePhoto;

public class EmptyTrashBinHandler : IRequestHandler<EmptyTrashBinRequest, EmptyTrashBinResponse>
{
    private readonly ITrashHardDeleteService _trashHardDelete;
    private readonly IApplicationConfigurationRepository _applicationConfigurationRepository;
    private readonly IDocumentSession _session;
    private readonly IMediator _mediatr;

    public EmptyTrashBinHandler(
        ITrashHardDeleteService trashHardDelete,
        IDocumentSession session,
        IMediator mediatr,
        IApplicationConfigurationRepository applicationConfigurationRepository)
    {
        _trashHardDelete = trashHardDelete;
        _applicationConfigurationRepository = applicationConfigurationRepository;
        _session = session;
        _mediatr = mediatr;
    }

    public async ValueTask<EmptyTrashBinResponse> Handle(EmptyTrashBinRequest request, CancellationToken cancellationToken)
    {
        var result = new EmptyTrashBinResponse { IsSuccess = false };

        var op_ids = await _trashHardDelete.EmptyTrashBin(request.UserId);

        await _mediatr.Send(new DeletePermanentRequest { ObjectIds = op_ids }, cancellationToken);

        var _applicationConfig = await _applicationConfigurationRepository.GetConfiguration();
        _applicationConfig.SetValue("TrashHardDeleteConfiguration.LastRun", DateTimeOffset.UtcNow);
        await _applicationConfigurationRepository.SaveConfiguration(_applicationConfig);

        result.IsSuccess = true;
        return result;
    }
}