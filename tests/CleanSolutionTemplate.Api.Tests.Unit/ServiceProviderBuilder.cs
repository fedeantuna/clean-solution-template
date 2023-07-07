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

        this.ReplaceLoggerWithInMemoryLogger();

        this.UnregisterActualHttpContextAccessor();
        this.SetupHttpContextAccessorMock();
    }

    public IServiceProvider Build() => this._services.BuildServiceProvider();

    private void ReplaceLoggerWithInMemoryLogger() =>
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

    private void UnregisterActualHttpContextAccessor()
    {
        var httpContextAccessor = this._services.Single(s => s.ServiceType == typeof(IHttpContextAccessor));
        this._services.Remove(httpContextAccessor);
    }

    private void SetupHttpContextAccessorMock()
    {
        var httpContextAccessorMock = new Mock<IHttpContextAccessor>();

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

        this._services.AddSingleton(httpContextAccessorMock.Object);
    }
}
