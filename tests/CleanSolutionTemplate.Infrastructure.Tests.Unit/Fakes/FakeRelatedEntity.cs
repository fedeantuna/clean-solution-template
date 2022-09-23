using CleanSolutionTemplate.Domain.Common;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;

public class FakeRelatedEntity : AuditableEntity
{
    public FakeRelatedEntity()
        : base(Guid.NewGuid())
    {
    }

    public FakeEntity? FakeEntity { get; set; }

    public FakeValueObject? FakeValueObject { get; set; }
}
