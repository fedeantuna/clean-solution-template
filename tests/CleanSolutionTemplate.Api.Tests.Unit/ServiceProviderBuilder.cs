using System.Security.Claims;
using CleanSolutionTemplate.Api.SerilogPolicies;
using FakeItEasy;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Sinks.InMemory;

namespace CleanSolutionTemplate.Api.Tests.Unit;

public class ServiceProviderBuilder
{
    private readonly IServiceCollection _services = new ServiceCollection();

    public ServiceProviderBuilder()
    {
        var configuration = new ConfigurationBuilder().Build();
        this._services.AddPresentationServices(configuration, false);

        this.SetupLoggerWithInMemoryLogger();

        this.SetupHttpContextAccessorFake();
    }

    public IServiceProvider Build() => this._services.BuildServiceProvider();

    private void SetupLoggerWithInMemoryLogger() =>
        this._services.AddLogging(builder =>
        {
            builder.ClearProviders();

            var logger = new LoggerConfiguration()
                .Destructure
                .UseSensitiveDataMasking()
                .WriteTo.InMemory()
                .MinimumLevel.Verbose()
                .CreateLogger();
            builder.AddSerilog(logger);
        });

    private void SetupHttpContextAccessorFake()
    {
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Testing.TestUserId),
            new(ClaimTypes.Email, Testing.TestUserEmail)
        };
        var claimsIdentity = new ClaimsIdentity(claims);
        var user = new ClaimsPrincipal(claimsIdentity);

        var httpContextFake = A.Fake<HttpContext>();
        A.CallTo(() => httpContextFake.User).Returns(user);

        var httpContextAccessorFake = this._services.ReplaceServiceWithFake<IHttpContextAccessor>(ServiceLifetime.Singleton);
        A.CallTo(() => httpContextAccessorFake.HttpContext).Returns(httpContextFake);
    }
}

public static class ServiceCollectionExtensions
{
    public static TService ReplaceServiceWithFake<TService>(this IServiceCollection services, ServiceLifetime serviceLifetime)
        where TService : class
    {
        var service = services.Single(sd => sd.ServiceType == typeof(TService));
        services.Remove(service);
        var replace = A.Fake<TService>();
        var serviceDescriptor = new ServiceDescriptor(typeof(TService), _ => replace, serviceLifetime);
        services.Add(serviceDescriptor);

        return replace;
    }
}
