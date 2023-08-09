using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Domain.Common;
using CleanSolutionTemplate.Infrastructure.Common;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FakeItEasy;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;

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
        var domainEventFake = A.Fake<DomainEvent>();
        var entity = new FakeEntity();

        entity.AddDomainEvent(domainEventFake);
        await this._fakeDbContext.FakeEntities.AddAsync(entity);

        var publisherMock = A.Fake<IPublisher>();

        // Act
        await publisherMock.DispatchDomainEvents(this._fakeDbContext);

        // Assert
        entity.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public async Task DispatchDomainEvents_ShouldPublishEachDomainEvent()
    {
        // Arrange
        var domainEventAFake = A.Fake<DomainEvent>();
        var domainEventBFake = A.Fake<DomainEvent>();
        var entity = new FakeEntity();

        var cancellationToken = new CancellationToken();

        entity.AddDomainEvent(domainEventAFake);
        entity.AddDomainEvent(domainEventBFake);
        await this._fakeDbContext.FakeEntities.AddAsync(entity, cancellationToken);

        var publisherFake = A.Fake<IPublisher>();

        // Act
        await publisherFake.DispatchDomainEvents(this._fakeDbContext, cancellationToken);

        // Assert
        A.CallTo(() => publisherFake.Publish(domainEventAFake, cancellationToken)).MustHaveHappenedOnceExactly();
        A.CallTo(() => publisherFake.Publish(domainEventBFake, cancellationToken)).MustHaveHappenedOnceExactly();
    }
}
