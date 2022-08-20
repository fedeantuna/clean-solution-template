using CleanSolutionTemplate.Domain.Common;

namespace CleanSolutionTemplate.Infrastructure.Tests.Unit.Fakes;

public class FakeValueObject : ValueObject
{
    private const int FakeValue = 0;

    protected override IEnumerable<object?> GetEqualityComponents()
    {
        yield return FakeValue;
    }
}