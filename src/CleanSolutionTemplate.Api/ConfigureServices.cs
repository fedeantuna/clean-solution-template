using System.Diagnostics.CodeAnalysis;
using CleanSolutionTemplate.Api.Services;
using CleanSolutionTemplate.Application.Common.Services;

namespace CleanSolutionTemplate.Api;

public static class ConfigureServices
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static IServiceCollection AddPresentationServices(this IServiceCollection services)
    {
        services.AddDatabaseDeveloperPageExceptionFilter();
        services.AddHttpContextAccessor();

        services.AddSingleton<IUserService, UserService>();

        return services;
    }
}
