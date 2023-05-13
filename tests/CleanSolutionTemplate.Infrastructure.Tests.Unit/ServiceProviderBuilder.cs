using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Application.Common.Services;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Infrastructure.Persistence;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Moq;

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

        this.AddPresentationServiceMocks();
        this.AddApplicationServiceMocks();

        this.SetupWrapperMocks();
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

    private void AddPresentationServiceMocks()
    {
        var userServiceMock = new Mock<IUserService>();
        userServiceMock.Setup(us => us.GetCurrentUserId()).Returns(Constants.TestUserId);
        userServiceMock.Setup(us => us.GetCurrentUserEmail()).Returns(Constants.TestUserEmail);
        this._services.AddTransient(_ => userServiceMock.Object);
    }

    private void AddApplicationServiceMocks()
    {
        var mediatorMock = new Mock<IMediator>();
        this._services.AddTransient(_ => mediatorMock.Object);

        var publisherMock = new Mock<IPublisher>();
        this._services.AddTransient(_ => publisherMock.Object);
    }

    private void SetupWrapperMocks()
    {
        var dateTimeOffsetWrapper = this._services.Single(s => s.ServiceType == typeof(IDateTimeOffsetWrapper));
        this._services.Remove(dateTimeOffsetWrapper);

        var dateTimeOffsetWrapperMock = new Mock<IDateTimeOffsetWrapper>();
        dateTimeOffsetWrapperMock.SetupGet(dow => dow.UtcNow).Returns(DateTimeOffset.UtcNow);
        this._services.AddSingleton(_ => dateTimeOffsetWrapperMock.Object);
    }
}
