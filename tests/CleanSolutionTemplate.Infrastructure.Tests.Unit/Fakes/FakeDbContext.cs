using Microsoft.EntityFrameworkCore;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;

public class FakeDbContext : DbContext
{
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseInMemoryDatabase("FakeDbContext");

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<FakeEntityWithValueObject>()
            .OwnsOne(e => e.FakeValueObject);
    }

    public DbSet<FakeEntity> FakeEntities => this.Set<FakeEntity>();

    public DbSet<FakeEntityWithValueObject> FakeEntitiesWithValueObject => this.Set<FakeEntityWithValueObject>();
}