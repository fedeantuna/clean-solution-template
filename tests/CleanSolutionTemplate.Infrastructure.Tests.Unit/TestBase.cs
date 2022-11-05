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

public class TestBase
{
    private const string InMemoryDatabaseName = "InMemoryDatabase";
    private const string TestUserEmail = "test-user-email";

    protected const string TestUserId = "test-user-id";

    private readonly IServiceCollection _services = new ServiceCollection();
    private readonly IServiceProvider _provider;

    protected TestBase()
    {
        this._services.AddInfrastructureServices(null!, true);
        this.ReplaceApplicationDbContextWithFakeDbContext();

        this._services.AddLogging();

        this.AddPresentationServiceMocks();
        this.AddApplicationServiceMocks();

        this.SetupWrapperMocks();

        this._provider = this._services.BuildServiceProvider();
    }

    protected DateTimeOffset UtcNow { get; } = DateTimeOffset.UtcNow;

    protected Mock<IPublisher> PublisherMock { get; private set; } = null!;

    protected T FindService<T>()
        where T : notnull
    {
        return this._provider.GetRequiredService<T>();
    }

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
        userServiceMock.Setup(us => us.GetCurrentUserId()).Returns(TestUserId);
        userServiceMock.Setup(us => us.GetCurrentUserEmail()).Returns(TestUserEmail);
        this._services.AddTransient(_ => userServiceMock.Object);
    }

    private void AddApplicationServiceMocks()
    {
        var mediatorMock = new Mock<IMediator>();
        this._services.AddTransient(_ => mediatorMock.Object);

        PublisherMock = new Mock<IPublisher>();
        this._services.AddTransient(_ => PublisherMock.Object);
    }

    private void SetupWrapperMocks()
    {
        var dateTimeOffsetWrapper = this._services.Single(s => s.ServiceType == typeof(IDateTimeOffsetWrapper));
        this._services.Remove(dateTimeOffsetWrapper);
        var dateTimeOffsetWrapperMock = new Mock<IDateTimeOffsetWrapper>();
        dateTimeOffsetWrapperMock.SetupGet(dow => dow.UtcNow).Returns(this.UtcNow);
        this._services.AddSingleton(_ => dateTimeOffsetWrapperMock.Object);
    }
}
