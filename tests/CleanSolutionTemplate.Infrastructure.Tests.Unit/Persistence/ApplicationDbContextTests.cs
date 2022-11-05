using CleanSolutionTemplate.Application.Common.Persistence;
using CleanSolutionTemplate.Application.Common.Wrappers;
using CleanSolutionTemplate.Domain.Common;
using CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;
using FluentAssertions;
using Moq;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Persistence;

public class ApplicationDbContextTests : TestBase
{
    private readonly IApplicationDbContext _sut;

    public ApplicationDbContextTests()
    {
        this._sut = this.FindService<IApplicationDbContext>();
    }

    [Fact]
    public async Task SaveChangesAsync_InvokesAuditableEntitySaveChangesInterceptor()
    {
        // Arrange
        var fakeEntity = new FakeEntity();

        await ((FakeDbContext)this._sut).FakeEntities.AddAsync(fakeEntity);

        var now = this.FindService<IDateTimeOffsetWrapper>().UtcNow;

        // Act
        await this._sut.SaveChangesAsync();

        // Assert
        fakeEntity.CreatedAt.Should().Be(now);
        fakeEntity.LastModifiedAt.Should().Be(now);
        fakeEntity.CreatedBy.Should().Be(TestUserId);
        fakeEntity.LastModifiedBy.Should().Be(TestUserId);
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

        // Act
        await this._sut.SaveChangesAsync(cancellationToken);

        // Assert
        this.PublisherMock.Verify(p => p.Publish(domainEvent, cancellationToken));
    }
}
