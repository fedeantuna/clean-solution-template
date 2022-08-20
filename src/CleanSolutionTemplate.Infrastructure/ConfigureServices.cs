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
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.ConfigurePersistence(configuration);
        services.ConfigureWrappers();

        return services;
    }

    private static void ConfigurePersistence(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<AuditableEntitySaveChangesInterceptor>();

        const string useInMemoryDatabaseSettingName = "UseInMemoryDatabase";
        if (configuration.GetValue<bool>(useInMemoryDatabaseSettingName))
        {
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseInMemoryDatabase("AuraDb"));
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
    }

    private static void ConfigureWrappers(this IServiceCollection services)
    {
        services.AddSingleton<IDateTimeOffsetWrapper, DateTimeOffsetWrapper>();
    }
}