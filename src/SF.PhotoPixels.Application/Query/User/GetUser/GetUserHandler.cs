using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;

namespace SF.PhotoPixels.Application.Query.User.GetUser
{
    public class GetUserHandler : IQueryHandler<GetUserRequest, OneOf<GetUserResponse, None>>
    {
        private readonly UserManager<Domain.Entities.User> _userManager;


        public GetUserHandler(UserManager<Domain.Entities.User> userManager)
        {
            _userManager = userManager;
        }

        public async ValueTask<OneOf<GetUserResponse, None>> Handle(GetUserRequest request, CancellationToken cancellationToken)
        {
            var result = await _userManager.FindByIdAsync(request.Id);

            if (result == null)
            {
                return new None();
            }

            return new GetUserResponse
            {
                Id = result.Id,
                DateCreated = result.DateCreated,
                Email = result.Email,
                Name = result.Name,
                UserName = result.UserName,
                Quota = result.Quota,
                UsedQuota = result.UsedQuota,
                Role = result.Role
            };
        }
    }
}
