using Mediator;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Infrastructure.Services.TusService;
using SolidTUS.Extensions;

namespace SF.PhotoPixels.Application.Commands.Tus.SendChunk;

public class UploadChunkHandler : IRequestHandler<UploadChunkRequest, UploadChunkResponse>
{
    private readonly IExecutionContextAccessor _contextAccessor;
    private readonly ITusService _tusService;

    public UploadChunkHandler(IExecutionContextAccessor executionContextAccessor, ITusService tusService) 
    {
        _contextAccessor = executionContextAccessor;
        _tusService = tusService;
    }

    public async ValueTask<UploadChunkResponse> Handle(UploadChunkRequest request, CancellationToken cancellationToken)
    {

        var ctx = _contextAccessor.HttpContext!.TusUpload(request.FileId).OnUploadFinished(_tusService.HandleNewCompletion).Build();

        await ctx.StartAppendDataAsync(_contextAccessor.HttpContext);

        return new UploadChunkResponse();
    }

}
