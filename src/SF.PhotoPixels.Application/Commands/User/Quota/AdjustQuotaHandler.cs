using Marten;
using Mediator;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Commands.User.Quota;

public class StorePhotoHandler : IRequestHandler<AdjustQuotaRequest, OneOf<AdjustQuotaResponse, NotFound, ValidationError>>
{
    private readonly IDocumentSession _session;

    public StorePhotoHandler(IDocumentSession session)
    {
        _session = session;
    }

    public async ValueTask<OneOf<AdjustQuotaResponse, NotFound, ValidationError>> Handle(AdjustQuotaRequest request, CancellationToken cancellationToken)
    {
        var user = await _session.Query<Domain.Entities.User>()
            .SingleOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        if (user == null)
        {
            return new NotFound();
        }

        if (user.UsedQuota >= request.Quota)
        {
            return new ValidationError(new Dictionary<string, string[]> { { "CannotChangeQuota", new[] { "Cannot reduce quota lower than the used quota" } } });
        }

        user.Quota = request.Quota;
        _session.Update(user);
        await _session.SaveChangesAsync(cancellationToken);

        return new AdjustQuotaResponse
        {
            Quota = user.Quota,
            UsedQuota = user.UsedQuota
        };
    }
}