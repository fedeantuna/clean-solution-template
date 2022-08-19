using CleanSolutionTemplate.Application.Common.Services;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using FluentValidation;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

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

        this.SetupFakeLogging();

        this.SetupUserServiceMock();

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

    private void SetupFakeLogging()
    {
        this._services.AddSingleton(typeof(ILogger<>), typeof(FakeLogger<>));
    }

    private void SetupUserServiceMock()
    {
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(us => us.GetCurrentUserId()).Returns(TestUserId);
        userServiceMock.Setup(us => us.GetCurrentUserEmail()).Returns(TestUserEmail);
        this._services.AddTransient(_ => userServiceMock.Object);
    }

    private void UnregisterActualValidators()
    {
        var validators = this._services.Where(s => s.ServiceType == typeof(IValidator<>)).ToList();
        validators.ForEach(validator => this._services.Remove(validator));
    }

    private void SetupValidatorsMock()
    {
        this.ValidatorAMock = new Mock<IValidator<IRequest<string>>>();
        this._services.AddScoped(_ => this.ValidatorAMock.Object);

        this.ValidatorBMock = new Mock<IValidator<IRequest<string>>>();
        this._services.AddScoped(_ => this.ValidatorBMock.Object);
    }
}