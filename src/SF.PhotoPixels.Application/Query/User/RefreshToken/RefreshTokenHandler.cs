using Mediator;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using OneOf;
using OneOf.Types;
using SF.PhotoPixels.Application.Security.BearerToken;

namespace SF.PhotoPixels.Application.Query.User.RefreshToken;

public class RefreshTokenHandler : IQueryHandler<RefreshTokenRequest, OneOf<Success, Unauthorized>>
{
    private readonly IOptionsMonitor<BearerTokenOptions> _bearerTokenOptions;
    private readonly SignInManager<Domain.Entities.User> _signInManager;

    public RefreshTokenHandler(IOptionsMonitor<BearerTokenOptions> bearerTokenOptions, SignInManager<Domain.Entities.User> signInManager)
    {
        _bearerTokenOptions = bearerTokenOptions;
        _signInManager = signInManager;
    }


    public async ValueTask<OneOf<Success, Unauthorized>> Handle(RefreshTokenRequest query, CancellationToken cancellationToken)
    {
        var refreshTokenProtector = _bearerTokenOptions.Get(BearerTokenDefaults.AuthenticationScheme).RefreshTokenProtector;
        var refreshTicket = refreshTokenProtector.Unprotect(query.RefreshToken);

        if (refreshTicket?.Properties.ExpiresUtc is not { } expiresUtc ||
            DateTime.UtcNow >= expiresUtc ||
            await _signInManager.ValidateSecurityStampAsync(refreshTicket.Principal) is not { } user)
        {
            return new Unauthorized();
        }

        _signInManager.AuthenticationScheme = BearerTokenDefaults.AuthenticationScheme;
        await _signInManager.SignInAsync(user, false, BearerTokenDefaults.AuthenticationScheme);

        return new Success();
    }
}