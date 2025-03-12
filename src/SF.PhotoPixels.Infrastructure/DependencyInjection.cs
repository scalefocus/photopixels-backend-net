using DbUp;
using HeyRed.ImageSharp.Heif.Formats.Heif;
using Marten;
using Marten.Events.Projections;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SF.PhotoPixels.Domain.Entities;
using SF.PhotoPixels.Domain.Events;
using SF.PhotoPixels.Domain.Repositories;
using SF.PhotoPixels.Infrastructure.Migrations;
using SF.PhotoPixels.Infrastructure.Helpers;
using SF.PhotoPixels.Infrastructure.Projections;
using SF.PhotoPixels.Infrastructure.Repositories;
using SF.PhotoPixels.Infrastructure.Storage;
using SF.PhotoPixels.Infrastructure.Stores;
using Weasel.Core;
using SF.PhotoPixels.Infrastructure.BackgroundServices.ImportDirectory;
using SF.PhotoPixels.Infrastructure.Services.PhotoService;
using SF.PhotoPixels.Infrastructure.Services.TusService;
using SixLabors.ImageSharp;
using SolidTUS.Extensions;
using SF.PhotoPixels.Infrastructure.BackgroundServices;
using SolidTUS.Models;

namespace SF.PhotoPixels.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration, bool isDevelopment)
    {
        services.AddScoped<IObjectStorage, LocalObjectStorage>();
        services.AddScoped<IPhotoService, PhotoService>();

        services.AddTransient<IObjectRepository, ObjectRepository>();

        services.AddIdentityCore<User>(opt => opt.User.RequireUniqueEmail = true);
        services.AddTransient<IUserStore<User>, UserStore>();
        services.AddTransient<IEmailSender, EmailSender>();
        services.AddTransient<IEmailGenerator, EmailGenerator>();

        services.AddHostedService<ImportDirectoryService>();
        services.AddTransient<IImportDirectoryService, ImportDirectoryService>();

        services.AddTransient<ITusService, TusService>();

        services.AddTus(opt =>
        {
            opt.HasTermination = true;
            opt.ExpirationStrategy = ExpirationStrategy.SlidingExpiration;
            opt.SlidingInterval = TimeSpan.FromDays(1);
            opt.ExpirationJobRunnerInterval = TimeSpan.FromDays(1);   
            opt.DeletePartialFilesOnMerge = true;
        })
        .FileStorageConfiguration(config =>
        {
            config.DirectoryPath = TusService.GetDirectory();
            config.MetaDirectoryPath = TusService.GetDirectory();
        })
        .AddExpirationHandler<TusExpirationHandler>()
        .WithExpirationJobRunner()
        .SetMetadataValidator(TusService.MetadataValidator)        
        .AllowEmptyMetadata(true);



        services.AddTransient<IApplicationConfigurationRepository, ApplicationConfigurationRepository>();

        var connectionString = configuration.GetConnectionString("PhotosMetadata")!;
        EnsureDatabase.For.PostgresqlDatabase(connectionString);

        var upgrader =
            DeployChanges.To
                .PostgresqlDatabase(connectionString, Constants.DefaultSchema)
                .WithScriptsEmbeddedInAssembly(typeof(DependencyInjection).Assembly, s => !s.Contains("drop") && s.EndsWith(".sql"))
                .WithScriptNameComparer(new EmbeddedMigrationScriptComparer())
                .WithVariablesDisabled()
                .LogToAutodetectedLog()
                .Build();

        upgrader.PerformUpgrade();

        var martenConfig = services.AddMarten(options =>
            {
                // Establish the connection string to your Marten database
                options.Connection(connectionString);
                options.DatabaseSchemaName = Constants.DefaultSchema;

                options.Schema.For<ObjectProperties>()
                    .SoftDeletedWithIndex()
                    .Index(x => x.Hash)
                    .Metadata(m => { m.IsSoftDeleted.MapTo(x => x.IsDeleted); })
                    .Duplicate(x => x.UserId, configure: idx => idx.IsUnique = false);

                options.Schema.For<User>()
                    .Index(x => x.Id);

                options.Schema.For<ApplicationConfiguration>()
                    .Index(x => x.Id);

                options.Projections.Add(new ObjectPropertiesProjection(), ProjectionLifecycle.Inline);

                options.AutoCreateSchemaObjects = AutoCreate.None;

                options.Events.AddEventType<MediaObjectCreated>();
                options.Events.AddEventType<MediaObjectUpdated>();
                options.Events.AddEventType<MediaObjectTrashed>();
                options.Events.AddEventType<MediaObjectRemovedFromTrash>();
                options.Events.AddEventType<MediaObjectDeleted>();
            })
            .UseLightweightSessions();

        if (!isDevelopment)
        {
            martenConfig.OptimizeArtifactWorkflow();
        }

        new HeifConfigurationModule().Configure(Configuration.Default);

        return services;
    }
}