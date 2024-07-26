using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using SF.PhotoPixels.Application.Security.BearerToken;
using SF.PhotoPixels.Domain.Entities;

namespace SF.PhotoPixels.Application.Security;

public class PrincipalFactory : IUserClaimsPrincipalFactory<User>
{
    public Task<ClaimsPrincipal> CreateAsync(User user)
    {
        var identity = new ClaimsIdentity(BearerTokenDefaults.AuthenticationScheme);

        identity.AddClaim(new Claim(CustomClaims.Id, user.Id.ToString()));
        identity.AddClaim(new Claim(CustomClaims.Email, user.Email));
        identity.AddClaim(new Claim(CustomClaims.FullName, user.Name));
        identity.AddClaim(new Claim(CustomClaims.Role, user.Role.ToString()));
        identity.AddClaim(new Claim(new ClaimsIdentityOptions().SecurityStampClaimType, user.SecurityStamp));

        return Task.FromResult(new ClaimsPrincipal(identity));
    }
}