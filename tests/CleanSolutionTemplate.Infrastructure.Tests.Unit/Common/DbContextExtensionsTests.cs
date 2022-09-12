using CleanSolutionTemplate.Infrastructure.Common;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Common;

public class DbContextExtensionsTests : TestBase
{
    [Fact]
    public void HasChangedOwnedEntities_ReturnsTrue_WhenEntryHasNewValueObject()
    {
        // Arrange
        var context = new FakeDbContext();

        context.FakeEntitiesWithValueObject.Add(new FakeEntityWithValueObject());

        var entry = context.ChangeTracker.Entries<FakeEntityWithValueObject>().Single();
        entry.State = EntityState.Added;

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasChangedOwnedEntities_ReturnsTrue_WhenEntryHasModifiedValueObject()
    {
        // Arrange
        var context = new FakeDbContext();

        context.FakeEntitiesWithValueObject.Add(new FakeEntityWithValueObject());

        var entry = context.ChangeTracker.Entries<FakeEntityWithValueObject>().Single();
        entry.State = EntityState.Modified;

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void HasChangedOwnedEntities_ReturnsFalse_WhenEntryDoesNotHaveNewNorModifiedValueObject()
    {
        // Arrange
        var context = new FakeDbContext();

        context.FakeEntitiesWithValueObject.Add(new FakeEntityWithValueObject());

        var entry = context.ChangeTracker.Entries<FakeEntityWithValueObject>().Single();
        var valueObjectEntry = context.ChangeTracker.Entries<FakeValueObject>().Single();
        valueObjectEntry.State = EntityState.Unchanged;

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void HasChangedOwnedEntities_ReturnsFalse_WhenEntryDoesNotHaveValueObject()
    {
        // Arrange
        var context = new FakeDbContext();

        context.FakeEntities.Add(new FakeEntity());

        var entry = context.ChangeTracker.Entries<FakeEntity>().Single();

        // Act
        var result = entry.HasChangedOwnedEntities();

        // Assert
        result.Should().BeFalse();
    }
}
