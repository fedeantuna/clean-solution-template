using System.Security.Claims;
using CleanSolutionTemplate.Api.SerilogPolicies;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
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

        this.SetupHttpContextAccessorMock();
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

    private void SetupHttpContextAccessorMock()
    {
        var httpContextAccessorMock = this._services.ReplaceServiceWithMock<IHttpContextAccessor>();

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, Testing.TestUserId),
            new(ClaimTypes.Email, Testing.TestUserEmail)
        };

        var claimsIdentity = new ClaimsIdentity(claims);

        var user = new ClaimsPrincipal(claimsIdentity);

        var httpContextMock = new Mock<HttpContext>();
        httpContextMock.SetupGet(hc => hc.User).Returns(user);

        httpContextAccessorMock.SetupGet(hca => hca.HttpContext).Returns(httpContextMock.Object);
    }
}

public static class ServiceCollectionExtensions
{
    public static Mock<TIService> ReplaceServiceWithMock<TIService>(this IServiceCollection services)
        where TIService : class
    {
        var service = services.Single(sd => sd.ServiceType == typeof(TIService));
        services.Remove(service);
        var replace = new Mock<TIService>();
        services.AddSingleton(_ => replace.Object);

        return replace;
    }
}
