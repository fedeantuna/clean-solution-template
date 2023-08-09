using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Application.Common.Services;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Infrastructure.Persistence;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FakeItEasy;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit;

public class ServiceProviderBuilder
{
    private const string InMemoryDatabaseName = "InMemoryDatabase";

    private readonly IServiceCollection _services = new ServiceCollection();

    public ServiceProviderBuilder()
    {
        this._services.AddInfrastructureServices(null!);
        this.ReplaceApplicationDbContextWithFakeDbContext();

        this._services.AddLogging();

        this.AddPresentationServiceFakes();
        this.AddApplicationServiceFakes();

        this.SetupWrapperFakes();
    }

    public IServiceProvider Build() => this._services.BuildServiceProvider();

    private void ReplaceApplicationDbContextWithFakeDbContext()
    {
        var applicationDbContextOptions = this._services.Single(sd =>
            sd.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
        var applicationDbContext = this._services.Single(sd =>
            sd.ServiceType == typeof(IApplicationDbContext));

        this._services.Remove(applicationDbContextOptions);
        this._services.Remove(applicationDbContext);

        this._services.AddDbContext<ApplicationDbContext>((_, options) =>
            options.UseInMemoryDatabase(InMemoryDatabaseName));
        this._services.AddScoped<IApplicationDbContext, FakeDbContext>();
    }

    private void AddPresentationServiceFakes()
    {
        var userServiceFake = A.Fake<IUserService>();
        A.CallTo(() => userServiceFake.GetCurrentUserId()).Returns(Testing.TestUserId);
        A.CallTo(() => userServiceFake.GetCurrentUserEmail()).Returns(Testing.TestUserEmail);
        this._services.AddTransient(_ => userServiceFake);
    }

    private void AddApplicationServiceFakes()
    {
        var mediatorFake = A.Fake<IMediator>();
        this._services.AddTransient(_ => mediatorFake);

        var publisherFake = A.Fake<IPublisher>();
        this._services.AddTransient(_ => publisherFake);
    }

    private void SetupWrapperFakes()
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
