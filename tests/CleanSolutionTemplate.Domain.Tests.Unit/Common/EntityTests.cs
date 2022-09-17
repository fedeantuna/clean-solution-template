using CleanSolutionTemplate.Domain.Common;
using FluentAssertions;
using Moq;

namespace CleanSolutionTemplate.Domain.Tests.Unit.Common;

public class EntityTests
{
    [Fact]
    public void Instance_ShouldRequireId()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var sut = new Mock<Entity>(id).Object;

        // Assert
        sut.Id.Should().NotBe(Guid.Empty);
    }

    [Fact]
    public void AddDomainEvent_ShouldAddDomainEvent()
    {
        // Arrange
        var id = Guid.NewGuid();
        var domainEvent = new Mock<DomainEvent>().Object;

        var entity = new Mock<Entity>(id).Object;

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
        var domainEvent = new Mock<DomainEvent>().Object;

        var entity = new Mock<Entity>(id).Object;
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.RemoveDomainEvent(domainEvent);

        // Assert
        entity.DomainEvents.Should().NotContain(domainEvent);
    }

    [Fact]
    public void RemoveRange_ShouldRemoveARangeDomainEvents()
    {
        // Arrange
        var id = Guid.NewGuid();
        var domainEvent = new Mock<DomainEvent>().Object;

        var entity = new Mock<Entity>(id).Object;
        entity.AddDomainEvent(domainEvent);

        // Act
        entity.RemoveRange(entity.DomainEvents.Count);

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }
}
