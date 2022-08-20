using CleanSolutionTemplate.Domain.Common;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;

public class FakeEntityWithValueObject : AuditableEntity
{
    public FakeEntityWithValueObject()
        : base(Guid.NewGuid())
    {
    }

    public FakeValueObject FakeValueObject { get; } = new();
}