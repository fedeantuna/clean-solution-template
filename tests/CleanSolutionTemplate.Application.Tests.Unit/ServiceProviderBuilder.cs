using System.Reflection;
using CleanSolutionTemplate.Application.Common.Services;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Sinks.InMemory;

namespace CleanSolutionTemplate.Application.Tests.Unit;

public class ServiceProviderBuilder
{
    private readonly IServiceCollection _services = new ServiceCollection();

    public ServiceProviderBuilder()
    {
        this._services.AddApplicationServices();
        this.AddFakeValidators();
        this.AddFakeMediatorRequests();

        this.SetupInMemoryLogger();
        this.AddPresentationServiceMocks();
    }

    public IServiceProvider Build() => this._services.BuildServiceProvider();

    private void AddFakeValidators() => this._services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

    private void AddFakeMediatorRequests() => this._services.AddMediatR(cfg =>
    {
        cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
    });

    private void SetupInMemoryLogger() =>
        this._services.AddLogging(builder =>
        {
            builder.ClearProviders();

            var logger = new LoggerConfiguration()
                .WriteTo.InMemory()
                .MinimumLevel.Verbose()
                .CreateLogger();
            builder.AddSerilog(logger);
        });

    private void AddPresentationServiceMocks()
    {
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(us => us.GetCurrentUserId()).Returns(Testing.TestUserId);
        userServiceMock.Setup(us => us.GetCurrentUserEmail()).Returns(Testing.TestUserEmail);
        this._services.AddTransient(_ => userServiceMock.Object);
    }
}
