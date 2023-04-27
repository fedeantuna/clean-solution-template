using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Domain.Common;
using CleanSolutionTemplate.Infrastructure.Common;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Common;

public class MediatorExtensionsTests
{
    private readonly FakeDbContext _fakeDbContext;

    public MediatorExtensionsTests()
    {
        var provider = new ServiceProviderBuilder().Build();

        this._fakeDbContext = (FakeDbContext)provider.GetRequiredService<IApplicationDbContext>();
    }

    [Fact]
    public async Task DispatchDomainEvents_ShouldClearTheDomainEventsFromTheEntity()
    {
        // Arrange
        var domainEventMock = new Mock<DomainEvent>();
        var entity = new FakeEntity();

        entity.AddDomainEvent(domainEventMock.Object);
        await this._fakeDbContext.FakeEntities.AddAsync(entity);

        var publisherMock = new Mock<IPublisher>();

        // Act
        await publisherMock.Object.DispatchDomainEvents(this._fakeDbContext);

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task DispatchDomainEvents_ShouldPublishEachDomainEvent()
    {
        // Arrange
        var domainEventAMock = new Mock<DomainEvent>();
        var domainEventBMock = new Mock<DomainEvent>();
        var entity = new FakeEntity();

        var cancellationToken = new CancellationToken();

        entity.AddDomainEvent(domainEventAMock.Object);
        entity.AddDomainEvent(domainEventBMock.Object);
        await this._fakeDbContext.FakeEntities.AddAsync(entity, cancellationToken);

        var publisherMock = new Mock<IPublisher>();

        // Act
        await publisherMock.Object.DispatchDomainEvents(this._fakeDbContext, cancellationToken);

        // Assert
        publisherMock.Verify(p => p.Publish(domainEventAMock.Object, cancellationToken), Times.Once);
        publisherMock.Verify(p => p.Publish(domainEventBMock.Object, cancellationToken), Times.Once);
    }
}
