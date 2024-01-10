using Microsoft.AspNetCore.Authorization;

namespace SF.PhotoPixels.API.Security.RequireAdminRole;

public class RequireAdminRoleRequirement : IAuthorizationRequirement
{
}