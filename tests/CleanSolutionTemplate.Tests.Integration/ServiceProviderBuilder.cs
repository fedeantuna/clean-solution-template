using CleanSolutionTemplate.Application;
using CleanSolutionTemplate.Application.Common.Services;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CleanSolutionTemplate.Tests.Integration;

public class ServiceProviderBuilder
{
    private readonly IServiceCollection _services = new ServiceCollection();

    public ServiceProviderBuilder()
    {
        var configuration = new ConfigurationBuilder()
            .AddJsonFile("appsettings.json", false)
            .Build();

        this._services.AddApplicationServices()
            .AddInfrastructureServices(configuration)
            .AddLogging();

        this.AddPresentationServiceMocks();

        this.SetupInfrastructureWrapperMocks();
    }

    public IServiceProvider Build() => this._services.BuildServiceProvider();

    private void AddPresentationServiceMocks()
    {
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(us => us.GetCurrentUserId()).Returns(Testing.TestUserId);
        userServiceMock.Setup(us => us.GetCurrentUserEmail()).Returns(Testing.TestUserEmail);
        this._services.AddTransient(_ => userServiceMock.Object);
    }

    private void SetupInfrastructureWrapperMocks()
    {
        var dateTimeOffsetWrapperMock = this._services.ReplaceServiceWithMock<IDateTimeOffsetWrapper>();
        dateTimeOffsetWrapperMock.SetupGet(dow => dow.UtcNow).Returns(DateTimeOffset.UtcNow);
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
