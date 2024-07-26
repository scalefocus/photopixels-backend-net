using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using SF.PhotoPixels.Application.Security;

namespace SF.PhotoPixels.Application.Core;

public class ExecutionContextAccessor : IExecutionContextAccessor
{
    private readonly IHttpContextAccessor? _httpContextAccessor;

    private string? _userIdString;

    protected string UserIdString => _userIdString ??= GetUserIdFromClaims();

    public bool IsAvailable => _httpContextAccessor?.HttpContext != null;

    public bool IsLoggedIn => _httpContextAccessor?.HttpContext?.User.Identity?.IsAuthenticated ?? false;

    public ClaimsPrincipal UserPrincipal => GetUserPrincipal() ?? new ClaimsPrincipal();

    public virtual Guid UserId
    {
        get => Guid.TryParse(UserIdString, out var result) ? result : Guid.Empty;
        set => _userIdString = value.ToString();
    }

    public HttpContext? HttpContext
    {
        get => _httpContextAccessor?.HttpContext;
        set
        {
            if (_httpContextAccessor != null)
            {
                _httpContextAccessor.HttpContext = value;
            }
        }
    }

    public ExecutionContextAccessor(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
        HttpContext = httpContextAccessor.HttpContext;
    }

    private string GetUserIdFromClaims()
    {
        var userId = _httpContextAccessor?.HttpContext?.User.FindFirstValue(CustomClaims.Id);

        return userId ?? string.Empty;
    }

    private ClaimsPrincipal? GetUserPrincipal()
    {
        return _httpContextAccessor?.HttpContext?.User;
    }
}