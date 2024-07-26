using Microsoft.AspNetCore.Authorization;
using SF.PhotoPixels.Application.Security;
using SF.PhotoPixels.Domain.Enums;

namespace SF.PhotoPixels.API.Security.RequireAdminRole;

public class RequireAdminRoleAuthorizationHandler : AuthorizationHandler<RequireAdminRoleRequirement>
{
    protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, RequireAdminRoleRequirement requirement)
    {
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Fail();

            return Task.CompletedTask;
        }

        var roleClaim = context.User.FindFirst(CustomClaims.Role);

        if (roleClaim == null)
        {
            context.Fail(new AuthorizationFailureReason(this, "User is not admin."));

            return Task.CompletedTask;
        }

        if (roleClaim.Value != Role.Admin.ToString())
        {
            context.Fail(new AuthorizationFailureReason(this, "User is not admin."));

            return Task.CompletedTask;
        }

        context.Succeed(requirement);

        return Task.CompletedTask;
    }
}