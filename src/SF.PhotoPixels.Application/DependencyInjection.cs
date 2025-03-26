using FluentValidation;
using Mediator;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using SF.PhotoPixels.Application.Config;
using SF.PhotoPixels.Application.Core;
using SF.PhotoPixels.Application.Pipelines;
using SF.PhotoPixels.Application.Query.PhotoStorage.LoadMedia;
using SF.PhotoPixels.Application.TrashHardDelete;
using SF.PhotoPixels.Application.VersionMigrations;
using SF.PhotoPixels.Domain.Enums;


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

        services.AddKeyedScoped<IMediaCreationHandler, VideoCreationHandler>(MediaType.Video);
        services.AddKeyedScoped<IMediaCreationHandler, PhotoCreationHandler>(MediaType.Photo);
        services.AddScoped<IMediaCreationFactory, MediaCreationFactory>();

        services.AddHostedService<TrashHardDeleteBackgroundService>();
        services.AddScoped<ITrashHardDeleteService, TrashHardDeleteService>();

        services.RegisterMigrations();

        services.AddValidatorsFromAssembly(typeof(DependencyInjection).Assembly);

        return services;
    }
}