using FluentValidation;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Pipelines;
using SF.PhotoPixels.Application.VersionMigrations;

namespace SF.PhotoPixels.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SystemConfig>(configuration.GetSection(SystemConfig.Name));

        services.AddHttpContextAccessor();

        services.TryAddScoped<IExecutionContextAccessor, ExecutionContextAccessor>();

        services.AddMediator(options => { options.ServiceLifetime = ServiceLifetime.Scoped; });
        services.AddScoped(typeof(IPipelineBehavior<,>), typeof(RequestValidator<,>));
        services.AddTransient<Initialization>();

        services.RegisterMigrations();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}