using System.Security.Claims;
using Microsoft.AspNetCore.Http;

namespace SF.PhotoPixels.Application.Core;

public interface IExecutionContextAccessor
{
    HttpContext? HttpContext { get; set; }

    bool IsAvailable { get; }

    /// <summary>
    ///     NOTE: if using the ExecutionContextAccessor from an external client (i.e. Azure Function Host)
    ///     then this value needs to be populated after initialisation from that platform
    /// </summary>
    Guid UserId { get; set; }

    ClaimsPrincipal UserPrincipal { get; }

    bool IsLoggedIn { get; }
}