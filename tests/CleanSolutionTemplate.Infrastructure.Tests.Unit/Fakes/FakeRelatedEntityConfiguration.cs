using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;

public class FakeRelatedEntityConfiguration : IEntityTypeConfiguration<FakeRelatedEntity>
{
    public void Configure(EntityTypeBuilder<FakeRelatedEntity> builder) =>
        builder.OwnsOne(fre =>
            fre.FakeValueObject);
}
