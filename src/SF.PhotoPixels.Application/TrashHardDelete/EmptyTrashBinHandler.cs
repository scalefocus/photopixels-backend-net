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
    private ApplicationConfiguration _applicationConfig;

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
        _applicationConfig = _applicationConfigurationRepository.GetConfiguration().Result;
    }

    public async ValueTask<EmptyTrashBinResponse> Handle(EmptyTrashBinRequest request, CancellationToken cancellationToken)
    {
        var result = new EmptyTrashBinResponse { IsSuccess = false };

        var op_ids = await _trashHardDelete.EmptyTrashBin(request.UserId);

        foreach (var id in op_ids)
        {
            await _mediatr.Send(new DeleteObjectRequest { Id = id }, cancellationToken);
        }

        _applicationConfig.SetValue("TrashHardDeleteConfiguration.LastRun", DateTimeOffset.UtcNow);
        await _applicationConfigurationRepository.SaveConfiguration(_applicationConfig);

        result.IsSuccess = true;
        return result;
    }
}