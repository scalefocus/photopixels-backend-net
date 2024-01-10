// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Net.Http.Headers;

namespace SF.PhotoPixels.Application.Security.BearerToken;

public sealed class BearerTokenHandler : SignInAuthenticationHandler<BearerTokenOptions>
{
    private static readonly AuthenticateResult FailedUnprotectingToken = AuthenticateResult.Fail("Unprotected token failed");
    private static readonly AuthenticateResult TokenExpired = AuthenticateResult.Fail("Token expired");

    private new BearerTokenEvents Events => (BearerTokenEvents)base.Events!;

    public BearerTokenHandler(IOptionsMonitor<BearerTokenOptions> optionsMonitor, ILoggerFactory loggerFactory, UrlEncoder urlEncoder, ISystemClock clock)
        : base(optionsMonitor, loggerFactory, urlEncoder, clock)
    {
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var messageReceivedContext = new MessageReceivedContext(Context, Scheme, Options);

        await Events.MessageReceivedAsync(messageReceivedContext);

        // ReSharper disable once ConditionIsAlwaysTrueOrFalseAccordingToNullableAPIContract
        if (messageReceivedContext.Result is not null)
        {
            return messageReceivedContext.Result;
        }

        var token = messageReceivedContext.Token ?? GetBearerTokenOrNull();

        if (token is null)
        {
            return AuthenticateResult.NoResult();
        }

        var ticket = Options.BearerTokenProtector.Unprotect(token);

        if (ticket?.Properties.ExpiresUtc is not { } expiresUtc)
        {
            return FailedUnprotectingToken;
        }

        if (Clock.UtcNow >= expiresUtc)
        {
            return TokenExpired;
        }

        return AuthenticateResult.Success(ticket);
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.Headers.Append(HeaderNames.WWWAuthenticate, "Bearer");

        return base.HandleChallengeAsync(properties);
    }

    protected override async Task HandleSignInAsync(ClaimsPrincipal user, AuthenticationProperties? properties)
    {
        var utcNow = Clock.UtcNow;

        properties ??= new AuthenticationProperties();
        properties.ExpiresUtc = utcNow + Options.BearerTokenExpiration;
        var id = user.Identities.First().Claims.First(x => x.Type.Equals("id")).Value;
        var response = new AccessTokenResponse
        {
            AccessToken = Options.BearerTokenProtector.Protect(CreateBearerTicket(user, properties)),
            ExpiresIn = (long)Options.BearerTokenExpiration.TotalSeconds,
            RefreshToken = Options.RefreshTokenProtector.Protect(CreateRefreshTicket(user, utcNow)),
            UserId = id,
        };

        Logger.AuthenticationSchemeSignedIn(Scheme.Name);

        await Context.Response.WriteAsJsonAsync(response);
    }

    // No-op to avoid interfering with any mass sign-out logic.
    protected override Task HandleSignOutAsync(AuthenticationProperties? properties)
    {
        return Task.CompletedTask;
    }

    private string? GetBearerTokenOrNull()
    {
        var authorization = Request.Headers.Authorization.ToString();

        return authorization.StartsWith("Bearer ", StringComparison.Ordinal)
            ? authorization["Bearer ".Length..]
            : null;
    }

    private AuthenticationTicket CreateBearerTicket(ClaimsPrincipal user, AuthenticationProperties properties)
    {
        return new AuthenticationTicket(user, properties, $"{Scheme.Name}:AccessToken");
    }

    private AuthenticationTicket CreateRefreshTicket(ClaimsPrincipal user, DateTimeOffset utcNow)
    {
        var refreshProperties = new AuthenticationProperties
        {
            ExpiresUtc = utcNow + Options.RefreshTokenExpiration,
        };

        return new AuthenticationTicket(user, refreshProperties, $"{Scheme.Name}:RefreshToken");
    }
}