using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using CleanSolutionTemplate.Application.Common.Behaviors;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

namespace CleanSolutionTemplate.Application;

public static class ConfigureServices
{
    [SuppressMessage("ReSharper", "UnusedMethodReturnValue.Global")]
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.ConfigureValidators();
        services.ConfigureMediator();

        return services;
    }

    private static void ConfigureValidators(this IServiceCollection services) =>
        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    private static void ConfigureMediator(this IServiceCollection services) =>
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());

            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        });
}
