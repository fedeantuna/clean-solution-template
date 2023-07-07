using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Domain.Common;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FluentAssertions;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Persistence;

public class ApplicationDbContextTests
{
    private readonly IDateTimeOffsetWrapper _dateTimeOffsetWrapper;
    private readonly IPublisher _publisher;

    private readonly IApplicationDbContext _sut;

    public ApplicationDbContextTests()
    {
        var provider = new ServiceProviderBuilder().Build();

        this._dateTimeOffsetWrapper = provider.GetRequiredService<IDateTimeOffsetWrapper>();
        this._publisher = provider.GetRequiredService<IPublisher>();

        this._sut = provider.GetRequiredService<IApplicationDbContext>();
    }

    [Fact]
    public async Task SaveChangesAsync_InvokesAuditableEntitySaveChangesInterceptor()
    {
        // Arrange
        var fakeEntity = new FakeEntity();

        await ((FakeDbContext)this._sut).FakeEntities.AddAsync(fakeEntity);

        var now = this._dateTimeOffsetWrapper.UtcNow;

        // Act
        await this._sut.SaveChangesAsync();

        // Assert
        fakeEntity.CreatedAt.Should().Be(now);
        fakeEntity.LastModifiedAt.Should().Be(now);
        fakeEntity.CreatedBy.Should().Be(Testing.TestUserId);
        fakeEntity.LastModifiedBy.Should().Be(Testing.TestUserId);
    }

    [Fact]
    public async Task SaveChangesAsync_DispatchesDomainEvents()
    {
        // Arrange
        var fakeEntity = new FakeEntity();
        var domainEvent = new Mock<DomainEvent>().Object;

        var cancellationToken = new CancellationToken();

        fakeEntity.AddDomainEvent(domainEvent);

        await ((FakeDbContext)this._sut).FakeEntities.AddAsync(fakeEntity, cancellationToken);

        var publisherMock = Mock.Get(this._publisher);

        // Act
        await this._sut.SaveChangesAsync(cancellationToken);

        // Assert
        publisherMock.Verify(p => p.Publish(domainEvent, cancellationToken), Times.Once);
    }
}
