using System.Reflection;
using CleanSolutionTemplate.Application.Common.Services;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Serilog;
using Serilog.Sinks.InMemory;

namespace CleanSolutionTemplate.Application.Tests.Unit;

public abstract class TestBase
{
    protected const string TestUserId = "test-user-id";
    protected const string TestUserEmail = "test-user-email";

    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly IServiceProvider _provider;

    protected TestBase()
    {
        this._services.AddApplicationServices();
        this.AddFakeMediatorRequests();

        this.SetupInMemoryLogger();
        this.AddPresentationServiceMocks();

        this.UnregisterActualValidators();
        this.SetupValidatorsMock();

        this._provider = this._services.BuildServiceProvider();
    }

    protected Mock<IValidator<IRequest<string>>> ValidatorAMock { get; private set; } = null!;
    protected Mock<IValidator<IRequest<string>>> ValidatorBMock { get; private set; } = null!;

    protected T FindService<T>()
        where T : notnull
    {
        return this._provider.GetRequiredService<T>();
    }

    private void AddFakeMediatorRequests()
    {
        this._services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });
    }

    private void SetupInMemoryLogger()
    {
        this._services.AddLogging(builder =>
        {
            builder.ClearProviders();

            var logger = new LoggerConfiguration()
                .WriteTo.InMemory()
                .MinimumLevel.Verbose()
                .CreateLogger();
            builder.AddSerilog(logger);
        });
    }

    private void AddPresentationServiceMocks()
    {
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(us => us.GetCurrentUserId()).Returns(TestUserId);
        userServiceMock.Setup(us => us.GetCurrentUserEmail()).Returns(TestUserEmail);
        this._services.AddTransient(_ => userServiceMock.Object);
    }

    private void UnregisterActualValidators()
    {
        var validators = this._services.Where(s => s.ServiceType == typeof(IValidator<>)).ToList();
        validators.ForEach(this.RemoveService);
    }

    private void SetupValidatorsMock()
    {
        this.ValidatorAMock = new Mock<IValidator<IRequest<string>>>();
        this._services.AddScoped(_ => this.ValidatorAMock.Object);

        this.ValidatorBMock = new Mock<IValidator<IRequest<string>>>();
        this._services.AddScoped(_ => this.ValidatorBMock.Object);
    }

    private void RemoveService(ServiceDescriptor serviceDescriptor) =>
        this._services.Remove(serviceDescriptor);
}
