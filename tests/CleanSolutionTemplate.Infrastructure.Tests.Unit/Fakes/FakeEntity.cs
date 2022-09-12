using CleanSolutionTemplate.Domain.Common;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;

public class FakeEntity : AuditableEntity
{
    public FakeEntity()
        : base(Guid.NewGuid())
    {
    }
}
