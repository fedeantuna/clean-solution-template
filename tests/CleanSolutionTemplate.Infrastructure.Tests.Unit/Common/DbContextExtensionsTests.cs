using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Infrastructure.Common;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Common;

public class DbContextExtensionsTests : TestBase
{
    private readonly FakeDbContext _fakeDbContext;

    public DbContextExtensionsTests()
    {
        this._fakeDbContext = (FakeDbContext)this.FindService<IApplicationDbContext>();
    }

    [Fact]
    public void HasChangedOwnedEntities_ReturnsTrue_WhenEntryHasNewValueObject()
    {
        // Arrange
        var entry = this.AddFakeRelatedEntityWithValueObject(EntityState.Added,
            EntityState.Added);

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasChangedOwnedEntities_ReturnsTrue_WhenEntryHasModifiedValueObject()
    {
        // Arrange
        var entry = this.AddFakeRelatedEntityWithValueObject(EntityState.Modified,
            EntityState.Added);

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasChangedOwnedEntities_ReturnsFalse_WhenEntryDoesNotHaveNewNorModifiedValueObject()
    {
        // Arrange
        var entry = this.AddFakeRelatedEntityWithValueObject(EntityState.Added,
            EntityState.Unchanged);

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasChangedOwnedEntities_ReturnsFalse_WhenEntryDoesNotHaveValueObject()
    {
        // Arrange
        var entry = this.AddFakeEntity();

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasChangedOwnedEntities_ReturnsFalse_WhenEntryHasAnotherEntityButDoesNotHaveValueObject()
    {
        // Arrange
        var entry = this.AddFakeRelatedEntityWithFakeEntity(EntityState.Added,
            EntityState.Added);

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeFalse();
    }

    private EntityEntry<T> SetEntryState<T>(EntityState state)
        where T : class
    {
        var entry = this._fakeDbContext.ChangeTracker.Entries<T>().Single();
        entry.State = state;

        return entry;
    }

    private EntityEntry<FakeRelatedEntity> AddFakeRelatedEntityWithValueObject(EntityState fakeRelatedEntityState,
        EntityState fakeValueObjectState)
    {
        this._fakeDbContext.FakeRelatedEntities.Add(new FakeRelatedEntity
        {
            FakeValueObject = new FakeValueObject()
        });

        this.SetEntryState<FakeValueObject>(fakeValueObjectState);
        var entry = this.SetEntryState<FakeRelatedEntity>(fakeRelatedEntityState);

        return entry;
    }

    private EntityEntry<FakeEntity> AddFakeEntity()
    {
        this._fakeDbContext.FakeEntities.Add(new FakeEntity());

        var entry = this.SetEntryState<FakeEntity>(EntityState.Added);

        return entry;
    }

    private EntityEntry<FakeRelatedEntity> AddFakeRelatedEntityWithFakeEntity(EntityState relatedEntityState,
        EntityState fakeEntityState)
    {
        this._fakeDbContext.FakeRelatedEntities.Add(new FakeRelatedEntity
        {
            FakeEntity = new FakeEntity()
        });

        this.SetEntryState<FakeEntity>(fakeEntityState);
        var entry = this.SetEntryState<FakeRelatedEntity>(relatedEntityState);

        return entry;
    }
}
