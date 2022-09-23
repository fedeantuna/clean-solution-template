using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Infrastructure.Persistence.Interceptors;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Moq;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Persistence.Interceptors;

public class AuditableEntitySaveChangesInterceptorTests : TestBase
{
    private readonly AuditableEntitySaveChangesInterceptor _sut;

    public AuditableEntitySaveChangesInterceptorTests()
    {
        this._sut = this.FindService<AuditableEntitySaveChangesInterceptor>();
    }

    [Fact]
    public async Task SavingChangesAsync_DoesNotThrow_WhenContextIsNull()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var context = (DbContext?)null;

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        var act = async () =>
            await this._sut.SavingChangesAsync(eventData,
                interceptionResult,
                cancellationToken);

        // Assert
        await act.Should().NotThrowAsync();
    }

    [Fact]
    public async Task SavingChangesAsync_UpdatesAuditableEntities_WhenEntitiesAreAdded()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var context = (FakeDbContext)this.FindService<IApplicationDbContext>();

        await context.FakeEntities.AddAsync(new FakeEntity(), cancellationToken);
        await context.FakeEntities.AddAsync(new FakeEntity(), cancellationToken);

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        await this._sut.SavingChangesAsync(eventData, interceptionResult, cancellationToken);

        // Assert
        var fakeEntities = context.FakeEntities.Local.ToList();
        fakeEntities.Select(fe => fe.CreatedBy).Should().Equal(TestUserId, TestUserId);
        fakeEntities.Select(fe => fe.LastModifiedBy).Should().Equal(TestUserId, TestUserId);
        fakeEntities.Select(fe => fe.CreatedAt).Should().Equal(this.UtcNow, this.UtcNow);
        fakeEntities.Select(fe => fe.LastModifiedAt).Should().Equal(this.UtcNow, this.UtcNow);
    }

    [Fact]
    public async Task SavingChangesAsync_DoesNotUpdatesAuditableEntities_WhenEntitiesAreNotAddedNorModified()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var context = (FakeDbContext)this.FindService<IApplicationDbContext>();

        await context.FakeEntities.AddAsync(new FakeEntity(), cancellationToken);
        await context.FakeEntities.AddAsync(new FakeEntity(), cancellationToken);
        context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Unchanged);

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        await this._sut.SavingChangesAsync(eventData, interceptionResult, cancellationToken);

        // Assert
        var fakeEntities = context.FakeEntities.Local.ToList();
        fakeEntities.Select(fe => fe.CreatedBy).Should().Equal(null, null);
        fakeEntities.Select(fe => fe.LastModifiedBy).Should().Equal(null, null);
        fakeEntities.Select(fe => fe.CreatedAt).Should().Equal((DateTimeOffset?)null, null);
        fakeEntities.Select(fe => fe.LastModifiedAt).Should().Equal((DateTimeOffset?)null, null);
    }

    [Fact]
    public async Task SavingChangesAsync_DoesNotUpdatesAuditableEntities_WhenValueObjectsAreNotModified()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var context = (FakeDbContext)this.FindService<IApplicationDbContext>();

        await context.FakeRelatedEntities.AddAsync(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        }, cancellationToken);
        await context.FakeRelatedEntities.AddAsync(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        }, cancellationToken);
        context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Unchanged);

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        await this._sut.SavingChangesAsync(eventData, interceptionResult, cancellationToken);

        // Assert
        var fakeEntitiesWithValueObject = context.FakeRelatedEntities.Local.ToList();
        fakeEntitiesWithValueObject.Select(fe => fe.CreatedBy).Should().Equal(null, null);
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedBy).Should().Equal(null, null);
        fakeEntitiesWithValueObject.Select(fe => fe.CreatedAt).Should().Equal((DateTimeOffset?)null, null);
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedAt).Should().Equal((DateTimeOffset?)null, null);
    }

    [Fact]
    public async Task SavingChangesAsync_UpdatesAuditableEntities_WhenValueObjectsAreModified()
    {
        // Arrange
        var cancellationToken = new CancellationToken();
        var context = (FakeDbContext)this.FindService<IApplicationDbContext>();

        await context.FakeRelatedEntities.AddAsync(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        }, cancellationToken);
        await context.FakeRelatedEntities.AddAsync(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        }, cancellationToken);
        context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Unchanged);
        context.ChangeTracker.Entries()
            .Where(e =>
                e.References.Any(r =>
                    r.TargetEntry != null &&
                    r.TargetEntry.Metadata.IsOwned()))
            .ToList()
            .ForEach(e => e.State = EntityState.Modified);

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        await this._sut.SavingChangesAsync(eventData, interceptionResult, cancellationToken);

        // Assert
        var fakeEntitiesWithValueObject = context.FakeRelatedEntities.Local.ToList();
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedBy).Should().Equal(TestUserId, TestUserId);
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedAt).Should().Equal(this.UtcNow, this.UtcNow);
    }

    [Fact]
    public void SavingChanges_DoesNotThrow_WhenContextIsNull()
    {
        // Arrange
        var context = (DbContext?)null;

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        var act = () => this._sut.SavingChanges(eventData, interceptionResult);

        // Assert
        act.Should().NotThrow();
    }

    [Fact]
    public void SavingChanges_UpdatesAuditableEntities_WhenEntitiesAreAdded()
    {
        // Arrange
        var context = (FakeDbContext)this.FindService<IApplicationDbContext>();

        context.FakeEntities.Add(new FakeEntity());
        context.FakeEntities.Add(new FakeEntity());

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        this._sut.SavingChanges(eventData, interceptionResult);

        // Assert
        var fakeEntities = context.FakeEntities.Local.ToList();
        fakeEntities.Select(fe => fe.CreatedBy).Should().Equal(TestUserId, TestUserId);
        fakeEntities.Select(fe => fe.LastModifiedBy).Should().Equal(TestUserId, TestUserId);
        fakeEntities.Select(fe => fe.CreatedAt).Should().Equal(this.UtcNow, this.UtcNow);
        fakeEntities.Select(fe => fe.LastModifiedAt).Should().Equal(this.UtcNow, this.UtcNow);
    }

    [Fact]
    public void SavingChanges_DoesNotUpdatesAuditableEntities_WhenEntitiesAreNotAddedNorModified()
    {
        // Arrange
        var context = (FakeDbContext)this.FindService<IApplicationDbContext>();

        context.FakeEntities.Add(new FakeEntity());
        context.FakeEntities.Add(new FakeEntity());
        context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Unchanged);

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        this._sut.SavingChanges(eventData, interceptionResult);

        // Assert
        var fakeEntities = context.FakeEntities.Local.ToList();
        fakeEntities.Select(fe => fe.CreatedBy).Should().Equal(null, null);
        fakeEntities.Select(fe => fe.LastModifiedBy).Should().Equal(null, null);
        fakeEntities.Select(fe => fe.CreatedAt).Should().Equal((DateTimeOffset?)null, null);
        fakeEntities.Select(fe => fe.LastModifiedAt).Should().Equal((DateTimeOffset?)null, null);
    }

    [Fact]
    public void SavingChanges_DoesNotUpdatesAuditableEntities_WhenValueObjectsAreNotModified()
    {
        // Arrange
        var context = (FakeDbContext)this.FindService<IApplicationDbContext>();

        context.FakeRelatedEntities.Add(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        });
        context.FakeRelatedEntities.Add(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        });
        context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Unchanged);

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        this._sut.SavingChanges(eventData, interceptionResult);

        // Assert
        var fakeEntitiesWithValueObject = context.FakeRelatedEntities.Local.ToList();
        fakeEntitiesWithValueObject.Select(fe => fe.CreatedBy).Should().Equal(null, null);
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedBy).Should().Equal(null, null);
        fakeEntitiesWithValueObject.Select(fe => fe.CreatedAt).Should().Equal((DateTimeOffset?)null, null);
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedAt).Should().Equal((DateTimeOffset?)null, null);
    }

    [Fact]
    public void SavingChanges_UpdatesAuditableEntities_WhenValueObjectsAreModified()
    {
        // Arrange
        var context = (FakeDbContext)this.FindService<IApplicationDbContext>();

        context.FakeRelatedEntities.Add(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        });
        context.FakeRelatedEntities.Add(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        });
        context.ChangeTracker.Entries().ToList().ForEach(e => e.State = EntityState.Unchanged);
        context.ChangeTracker.Entries()
            .Where(e =>
                e.References.Any(r =>
                    r.TargetEntry != null &&
                    r.TargetEntry.Metadata.IsOwned()))
            .ToList()
            .ForEach(e => e.State = EntityState.Modified);

        var eventDefinition = CreateEventDefinition();
        var eventData = CreateDbContextEventData(eventDefinition, context);
        var interceptionResult = new InterceptionResult<int>();

        // Act
        this._sut.SavingChanges(eventData, interceptionResult);

        // Assert
        var fakeEntitiesWithValueObject = context.FakeRelatedEntities.Local.ToList();
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedBy).Should().Equal(TestUserId, TestUserId);
        fakeEntitiesWithValueObject.Select(fe => fe.LastModifiedAt).Should().Equal(this.UtcNow, this.UtcNow);
    }

    private static EventDefinition CreateEventDefinition()
    {
        var loggingOptionsMock = new Mock<ILoggingOptions>();
        var eventId = new EventId(0);
        const LogLevel logLevel = LogLevel.None;
        const string eventIdCode = "test-event-id-code";
        Action<ILogger, Exception?> LogActionFunc(LogLevel _) => (_, _) => { };

        return new EventDefinition(loggingOptionsMock.Object,
            eventId,
            logLevel,
            eventIdCode,
            LogActionFunc);
    }

    private static DbContextEventData CreateDbContextEventData(EventDefinitionBase eventDefinition, DbContext? context)
    {
        string MessageGenerator(EventDefinitionBase eventDefinitionBase, EventData eventData) => string.Empty;

        return new DbContextEventData(eventDefinition,
            MessageGenerator,
            context);
    }
}
