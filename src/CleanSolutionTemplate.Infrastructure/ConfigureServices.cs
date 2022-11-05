using System.Diagnostics.CodeAnalysis;
using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Infrastructure.Persistence;
using CleanSolutionTemplate.Infrastructure.Persistence.Interceptors;
using CleanSolutionTemplate.Infrastructure.Wrappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanSolutionTemplate.Infrastructure;

public static class ConfigureServices
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration,
        bool useInMemoryDatabase)
    {
        services.ConfigurePersistence(configuration, useInMemoryDatabase);
        services.ConfigureWrappers();

        return services;
    }

    private static void ConfigurePersistence(this IServiceCollection services,
        IConfiguration configuration,
        bool useInMemoryDatabase)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        if (useInMemoryDatabase)
        {
            const string inMemoryDatabaseName = "CleanSolutionTemplateDb";
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase(inMemoryDatabaseName));
        }
        else
        {
            const string cosmosAccountEndpointSettingName = "Cosmos:AccountEndpoint";
            const string cosmosAccountKeySettingName = "Cosmos:AccountKey";
            const string cosmosDatabaseNameSettingName = "Cosmos:DatabaseName";
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseCosmos(configuration.GetValue<string>(cosmosAccountEndpointSettingName),
                    configuration.GetValue<string>(cosmosAccountKeySettingName),
                    configuration.GetValue<string>(cosmosDatabaseNameSettingName)));
        }

        services.AddScoped<IApplicationDbContext>(provider => provider.GetRequiredService<ApplicationDbContext>());

        services.AddHealthChecks()
            .AddDbContextCheck<ApplicationDbContext>();
    }

    private static void ConfigureWrappers(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeOffsetWrapper, DateTimeOffsetWrapper>();
    }
}
