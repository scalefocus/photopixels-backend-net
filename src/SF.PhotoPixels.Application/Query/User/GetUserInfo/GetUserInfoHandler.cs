using Mediator;
using Microsoft.AspNetCore.Identity;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Core;

namespace SF.PhotoPixels.Application.Query.User.GetUserInfo;

public class GetUserInfoHandler : IQueryHandler<GetUserInfoRequest, OneOf<GetUserInfoResponse, NotFound>>
{
    private readonly IExecutionContextAccessor _executionContextAccessor;
    private readonly UserManager<Domain.Entities.User> _userManager;

    public GetUserInfoHandler(UserManager<Domain.Entities.User> userManager, IExecutionContextAccessor executionContextAccessor)
    {
        _userManager = userManager;
        _executionContextAccessor = executionContextAccessor;
    }

    public async ValueTask<OneOf<GetUserInfoResponse, NotFound>> Handle(GetUserInfoRequest query, CancellationToken cancellationToken)
    {
        var claimsPrincipal = _executionContextAccessor.UserPrincipal;

        var user = await _userManager.GetUserAsync(claimsPrincipal);

        if (user is null)
        {
            return new NotFound();
        }

        return new GetUserInfoResponse
        {
            Email = await _userManager.GetEmailAsync(user) ?? throw new NotSupportedException("Users must have an email."),
            IsEmailConfirmed = await _userManager.IsEmailConfirmedAsync(user),
            Quota = user.Quota,
            UsedQuota = user.UsedQuota,
            Claims = claimsPrincipal.Claims.ToDictionary(c => c.Type, c => c.Value),
        };
    }
}