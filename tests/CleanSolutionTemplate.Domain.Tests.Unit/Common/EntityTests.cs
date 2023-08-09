using CleanSolutionTemplate.Domain.Common;
using FakeItEasy;
using FluentAssertions;

namespace CleanSolutionTemplate.Domain.Tests.Unit.Common;

public class EntityTests
{
    [Fact]
    public void Instance_ShouldRequireId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var sut = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            id
        }));

        // Assert
        sut.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddDomainEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var domainEvent = A.Fake<DomainEvent>();

        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            id
        }));

        // Act
        entity.AddDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().Contain(domainEvent);
    }

    [Fact]
    public void RemoveDomainEvent_ShouldRemoveDomainEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var domainEvent = A.Fake<DomainEvent>();

        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            id
        }));
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.RemoveDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().NotContain(domainEvent);
    }

    [Fact]
    public void ClearDomainEvents_ShouldRemoveAllDomainEvents()
    {
        // Arrange
        var id = Guid.NewGuid();
        var domainEvent = A.Fake<DomainEvent>();

        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            id
        }));
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.ClearDomainEvents();

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}
