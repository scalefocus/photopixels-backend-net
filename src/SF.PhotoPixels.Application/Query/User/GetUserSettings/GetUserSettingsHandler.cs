using Mediator;
using Microsoft.AspNetCore.Identity;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Query.User.GetUserSettings;

namespace SF.PhotoPixels.Application.Query.User.GetUserSettings
{
    public class GetUserSettingsHandler : IQueryHandler<GetUserSettingsRequest, GetUserSettingsResponse>
    {
        private readonly UserManager<Domain.Entities.User> _userManager;
        private readonly IExecutionContextAccessor _executionContextAccessor;

        public GetUserSettingsHandler(UserManager<Domain.Entities.User> userManager, IExecutionContextAccessor executionContextAccessor)
        {
            _userManager = userManager;
            _executionContextAccessor = executionContextAccessor;
        }


        public async ValueTask<GetUserSettingsResponse> Handle(GetUserSettingsRequest request, CancellationToken cancellationToken)
        {
            var userId = _executionContextAccessor.UserId.ToString();
            var user = await _userManager.FindByIdAsync(userId);

            return new GetUserSettingsResponse()
            {
                UserId = user.Id,
                Settings = user.Settings
            };
        }
    }
}
