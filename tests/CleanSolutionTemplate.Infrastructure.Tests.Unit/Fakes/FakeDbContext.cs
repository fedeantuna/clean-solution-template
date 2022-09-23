using CleanSolutionTemplate.Infrastructure.Persistence;
using CleanSolutionTemplate.Infrastructure.Persistence.Interceptors;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;

internal class FakeDbContext : ApplicationDbContext
{
    public FakeDbContext(DbContextOptions<ApplicationDbContext> options,
        IPublisher publisher,
        AuditableEntitySaveChangesInterceptor auditableEntitySaveChangesInterceptor)
        : base(options, publisher, auditableEntitySaveChangesInterceptor)
    {
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FakeRelatedEntity>()
            .OwnsOne(e =>
                e.FakeValueObject);
    }

    public DbSet<FakeEntity> FakeEntities =>
        this.Set<FakeEntity>();

    public DbSet<FakeRelatedEntity> FakeRelatedEntities =>
        this.Set<FakeRelatedEntity>();
}
