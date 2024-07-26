using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Options;

namespace SF.PhotoPixels.Application.Security.BearerToken;

public static class BearerTokenExtensions
{
    public static AuthenticationBuilder AddCustomBearerToken(this AuthenticationBuilder builder, Action<BearerTokenOptions> configure)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(configure);

        builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<BearerTokenOptions>, BearerTokenConfigureOptions>());

        return builder.AddScheme<BearerTokenOptions, BearerTokenHandler>(BearerTokenDefaults.AuthenticationScheme, configure);
    }
}