using CleanSolutionTemplate.Application;
using CleanSolutionTemplate.Application.Common.Services;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Infrastructure;
using FakeItEasy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

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

        this.AddPresentationServiceFakes();

        this.SetupInfrastructureWrapperFakes();
    }

    public IServiceProvider Build() => this._services.BuildServiceProvider();

    private void AddPresentationServiceFakes()
    {
        var userServiceMock = A.Fake<IUserService>();
        A.CallTo(() => userServiceMock.GetCurrentUserId()).Returns(Testing.TestUserId);
        A.CallTo(() => userServiceMock.GetCurrentUserEmail()).Returns(Testing.TestUserEmail);
        this._services.AddTransient(_ => userServiceMock);
    }

    private void SetupInfrastructureWrapperFakes()
    {
        var dateTimeOffsetWrapperMock = this._services.ReplaceServiceWithFake<IDateTimeOffsetWrapper>(ServiceLifetime.Transient);
        A.CallTo(() => dateTimeOffsetWrapperMock.UtcNow).Returns(Testing.UtcNow);
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
