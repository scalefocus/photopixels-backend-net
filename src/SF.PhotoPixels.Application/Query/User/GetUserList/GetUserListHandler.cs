using Marten;
using Mediator;

namespace SF.PhotoPixels.Application.Query.User.GetUserList
{
    public class GetUserListHandler : IQueryHandler<GetUserListRequest, IReadOnlyList<GetUserResponse>>
    {
        private readonly IDocumentSession _session;

        public GetUserListHandler(IDocumentSession session)
        {
            _session = session;
        }

        public async ValueTask<IReadOnlyList<GetUserResponse>> Handle(GetUserListRequest request, CancellationToken cancellationToken)
        {
            return await _session.Query<Domain.Entities.User>()
                .Select(item => new GetUserResponse
                {
                    Id = item.Id,
                    DateCreated = item.DateCreated,
                    Email = item.Email,
                    Name = item.Name,
                    UserName = item.UserName,
                    Quota = item.Quota,
                    UsedQuota = item.UsedQuota,
                    Role = item.Role
                })
                .ToListAsync(cancellationToken);
        }
    }
}