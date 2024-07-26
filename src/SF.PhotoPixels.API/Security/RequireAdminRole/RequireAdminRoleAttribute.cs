using Microsoft.AspNetCore.Authorization;

namespace SF.PhotoPixels.API.Security.RequireAdminRole;

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
public class RequireAdminRoleAttribute : AuthorizeAttribute
{
    public static string PolicyName = "RequireAdminRole";

    public RequireAdminRoleAttribute() : base(PolicyName)
    {
    }
}