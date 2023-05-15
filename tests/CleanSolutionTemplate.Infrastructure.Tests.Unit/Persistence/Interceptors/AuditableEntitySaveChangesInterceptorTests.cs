using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Infrastructure.Persistence.Interceptors;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Persistence.Interceptors;

public class AuditableEntitySaveChangesInterceptorTests
{
    private readonly IDateTimeOffsetWrapper _dateTimeOffsetWrapper;
    private readonly FakeDbContext _fakeDbContext;

    private readonly AuditableEntitySaveChangesInterceptor _sut;

    public AuditableEntitySaveChangesInterceptorTests()
    {
        var provider = new ServiceProviderBuilder().Build();

        this._fakeDbContext = (FakeDbContext)provider.GetRequiredService<IApplicationDbContext>();
        this._dateTimeOffsetWrapper = provider.GetRequiredService<IDateTimeOffsetWrapper>();

        this._sut = provider.GetRequiredService<AuditableEntitySaveChangesInterceptor>();
    }

    [Fact]
    public async Task SavingChangesAsync_DoesNotThrow_WhenContextIsNull()
    {
        // Act
        var act = async () =>
            await this.CallSavingChangesAsyncWithNullContext();

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SavingChangesAsync_UpdatesAuditableEntities_WhenEntitiesAreAdded()
    {
        // Arrange
        await this.AddFakeEntitiesAsync(2);

        // Act
        await this.CallSavingChangesAsync();

        // Assert
        this.VerifyFakeEntities(new[] { Constants.TestUserId, Constants.TestUserId },
            new[] { Constants.TestUserId, Constants.TestUserId },
            new DateTimeOffset?[] { this._dateTimeOffsetWrapper.UtcNow, this._dateTimeOffsetWrapper.UtcNow },
            new DateTimeOffset?[] { this._dateTimeOffsetWrapper.UtcNow, this._dateTimeOffsetWrapper.UtcNow });
    }

    [Fact]
    public async Task SavingChangesAsync_DoesNotUpdatesAuditableEntities_WhenEntitiesAreNotAddedNorModified()
    {
        // Arrange
        await this.AddFakeEntitiesAsync(2);
        this._fakeDbContext.ChangeTracker
            .Entries()
            .ToList()
            .ForEach(e =>
                e.State = EntityState.Unchanged);

        // Act
        await this.CallSavingChangesAsync();

        // Assert
        this.VerifyFakeEntities(new string?[] { null, null },
            new string?[] { null, null },
            new DateTimeOffset?[] { null, null },
            new DateTimeOffset?[] { null, null });
    }

    [Fact]
    public async Task SavingChangesAsync_DoesNotUpdatesAuditableEntities_WhenValueObjectsAreNotModified()
    {
        // Arrange
        await this.AddFakeRelatedEntitiesAsync(2);
        this._fakeDbContext.ChangeTracker
            .Entries()
            .ToList()
            .ForEach(e =>
                e.State = EntityState.Unchanged);

        // Act
        await this.CallSavingChangesAsync();

        // Assert
        this.VerifyFakeEntitiesWithValueObjects(new string?[] { null, null },
            new string?[] { null, null },
            new DateTimeOffset?[] { null, null },
            new DateTimeOffset?[] { null, null });
    }

    [Fact]
    public async Task SavingChangesAsync_UpdatesAuditableEntities_WhenValueObjectsAreModified()
    {
        // Arrange
        await this.AddFakeRelatedEntitiesAsync(2);
        this._fakeDbContext.ChangeTracker
            .Entries()
            .ToList()
            .ForEach(e =>
                e.State = EntityState.Unchanged);
        this._fakeDbContext.ChangeTracker
            .Entries()
            .Where(e =>
                e.References.Any(r =>
                    r.TargetEntry != null &&
                    r.TargetEntry.Metadata.IsOwned()))
            .ToList()
            .ForEach(e =>
                e.State = EntityState.Modified);

        // Act
        await this.CallSavingChangesAsync();

        // Assert
        this.VerifyFakeEntitiesWithValueObjects(new string?[] { null, null },
            new[] { Constants.TestUserId, Constants.TestUserId },
            new DateTimeOffset?[] { null, null },
            new DateTimeOffset?[] { this._dateTimeOffsetWrapper.UtcNow, this._dateTimeOffsetWrapper.UtcNow });
    }

    [Fact]
    public void SavingChanges_DoesNotThrow_WhenContextIsNull()
    {
        // Act
        var act = this.CallSavingChangesWithNullContext;

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SavingChanges_UpdatesAuditableEntities_WhenEntitiesAreAdded()
    {
        // Arrange
        this.AddFakeEntities(2);

        // Act
        this.CallSavingChanges();

        // Assert
        this.VerifyFakeEntities(new[] { Constants.TestUserId, Constants.TestUserId },
            new[] { Constants.TestUserId, Constants.TestUserId },
            new DateTimeOffset?[] { this._dateTimeOffsetWrapper.UtcNow, this._dateTimeOffsetWrapper.UtcNow },
            new DateTimeOffset?[] { this._dateTimeOffsetWrapper.UtcNow, this._dateTimeOffsetWrapper.UtcNow });
    }

    [Fact]
    public void SavingChanges_DoesNotUpdatesAuditableEntities_WhenEntitiesAreNotAddedNorModified()
    {
        // Arrange
        this.AddFakeEntities(2);
        this._fakeDbContext.ChangeTracker
            .Entries()
            .ToList()
            .ForEach(e =>
                e.State = EntityState.Unchanged);

        // Act
        this.CallSavingChanges();

        // Assert
        this.VerifyFakeEntities(new string?[] { null, null },
            new string?[] { null, null },
            new DateTimeOffset?[] { null, null },
            new DateTimeOffset?[] { null, null });
    }

    [Fact]
    public void SavingChanges_DoesNotUpdatesAuditableEntities_WhenValueObjectsAreNotModified()
    {
        // Arrange
        this.AddFakeRelatedEntities(2);
        this._fakeDbContext.ChangeTracker
            .Entries()
            .ToList()
            .ForEach(e =>
                e.State = EntityState.Unchanged);

        // Act
        this.CallSavingChanges();

        // Assert
        this.VerifyFakeEntitiesWithValueObjects(new string?[] { null, null },
            new string?[] { null, null },
            new DateTimeOffset?[] { null, null },
            new DateTimeOffset?[] { null, null });
    }

    [Fact]
    public void SavingChanges_UpdatesAuditableEntities_WhenValueObjectsAreModified()
    {
        // Arrange
        this.AddFakeRelatedEntities(2);
        this._fakeDbContext.ChangeTracker
            .Entries()
            .ToList()
            .ForEach(e =>
                e.State = EntityState.Unchanged);
        this._fakeDbContext.ChangeTracker
            .Entries()
            .Where(e =>
                e.References.Any(r =>
                    r.TargetEntry != null &&
                    r.TargetEntry.Metadata.IsOwned()))
            .ToList()
            .ForEach(e =>
                e.State = EntityState.Modified);

        // Act
        this.CallSavingChanges();

        // Assert
        this.VerifyFakeEntitiesWithValueObjects(new string?[] { null, null },
            new[] { Constants.TestUserId, Constants.TestUserId },
            new DateTimeOffset?[] { null, null },
            new DateTimeOffset?[] { this._dateTimeOffsetWrapper.UtcNow, this._dateTimeOffsetWrapper.UtcNow });
    }

    private static (DbContextEventData, InterceptionResult<int>) CreateSavingChangesParameters(DbContext? context)
    {
        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        return (eventData, interceptionResult);
    }

    private static EventDefinition CreateEventDefinition()
    {
        var loggingOptionsMock = new Mock<ILoggingOptions>();
        var eventId = new EventId(0);
        const LogLevel logLevelNone = LogLevel.None;
        const string eventIdCode = "test-event-id-code";

        Action<ILogger, Exception?> LogActionFunc(LogLevel logLevel)
        {
            return (logger, exception) => logger.Log(logLevel, exception, "some-message");
        }

        return new EventDefinition(loggingOptionsMock.Object,
            eventId,
            logLevelNone,
            eventIdCode,
            LogActionFunc);
    }

    private static DbContextEventData CreateDbContextEventData(EventDefinitionBase eventDefinition, DbContext? context)
    {
        string MessageGenerator(EventDefinitionBase eventDefinitionBase, EventData eventData)
        {
            return $"{eventDefinitionBase.EventIdCode}-{eventData.EventIdCode}";
        }

        return new DbContextEventData(eventDefinition,
            MessageGenerator,
            context);
    }

    private async Task AddFakeEntitiesAsync(int count)
    {
        for (var i = 0; i < count; i++) await this._fakeDbContext.FakeEntities.AddAsync(new FakeEntity());
    }

    private void AddFakeEntities(int count)
    {
        for (var i = 0; i < count; i++) this._fakeDbContext.FakeEntities.Add(new FakeEntity());
    }

    private async Task AddFakeRelatedEntitiesAsync(int count)
    {
        for (var i = 0; i < count; i++)
            await this._fakeDbContext.FakeRelatedEntities.AddAsync(new FakeRelatedEntity
            {
                FakeValueObject = new FakeValueObject()
            });
    }

    private void AddFakeRelatedEntities(int count)
    {
        for (var i = 0; i < count; i++)
            this._fakeDbContext.FakeRelatedEntities.Add(new FakeRelatedEntity
            {
                FakeValueObject = new FakeValueObject()
            });
    }

    private async Task CallSavingChangesAsync()
    {
        var (eventData, interceptionResult) = CreateSavingChangesParameters(this._fakeDbContext);

        await this._sut.SavingChangesAsync(eventData, interceptionResult);
    }

    private async Task CallSavingChangesAsyncWithNullContext()
    {
        var (eventData, interceptionResult) = CreateSavingChangesParameters(null);

        await this._sut.SavingChangesAsync(eventData, interceptionResult);
    }

    private void CallSavingChanges()
    {
        var (eventData, interceptionResult) = CreateSavingChangesParameters(this._fakeDbContext);

        this._sut.SavingChanges(eventData, interceptionResult);
    }

    private void CallSavingChangesWithNullContext()
    {
        var (eventData, interceptionResult) = CreateSavingChangesParameters(null);

        this._sut.SavingChanges(eventData, interceptionResult);
    }

    private void VerifyFakeEntities(string?[] createdBy,
        string?[] lastModifiedBy,
        DateTimeOffset?[] createdAt,
        DateTimeOffset?[] lastModifiedAt)
    {
        var fakeEntities = this._fakeDbContext.FakeEntities.Local.ToList();
        fakeEntities.Select(fe => fe.CreatedBy).Should().Equal(createdBy);
        fakeEntities.Select(fe => fe.LastModifiedBy).Should().Equal(lastModifiedBy);
        fakeEntities.Select(fe => fe.CreatedAt).Should().Equal(createdAt);
        fakeEntities.Select(fe => fe.LastModifiedAt).Should().Equal(lastModifiedAt);
    }

    private void VerifyFakeEntitiesWithValueObjects(string?[] createdBy,
        string?[] lastModifiedBy,
        DateTimeOffset?[] createdAt,
        DateTimeOffset?[] lastModifiedAt)
    {
        var fakeEntitiesWithValueObject = this._fakeDbContext.FakeRelatedEntities.Local.ToList();
        fakeEntitiesWithValueObject.Select(fe => fe.CreatedBy).Should().Equal(createdBy);
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedBy).Should().Equal(lastModifiedBy);
        fakeEntitiesWithValueObject.Select(fe => fe.CreatedAt).Should().Equal(createdAt);
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedAt).Should().Equal(lastModifiedAt);
    }
}
