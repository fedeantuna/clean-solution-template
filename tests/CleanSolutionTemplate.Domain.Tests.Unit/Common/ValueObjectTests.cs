using CleanSolutionTemplate.Domain.Common;
using FluentAssertions;

namespace CleanSolutionTemplate.Domain.Tests.Unit.Common;

public class ValueObjectTests
{
    private static readonly ValueObject APrettyValueObject = new ValueObjectA(1,
        "2",
        Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0"));

    [Theory]
    [MemberData(nameof(EqualValueObjects))]
    public void Equals_ShouldReturnTrue_WhenEqualValueObjects(ValueObject instanceA, ValueObject instanceB, string reason)
    {
        // Act
        var sut = instanceA.Equals(instanceB);

        // Assert
        sut.Should().BeTrue(reason);
    }

    [Theory]
    [MemberData(nameof(EqualValueObjects))]
    public void EqualOperator_ShouldReturnTrue_WhenEqualValueObjects(ValueObject instanceA, ValueObject instanceB, string reason)
    {
        // Act
        var sut = instanceA == instanceB;

        // Assert
        sut.Should().BeTrue(reason);
    }

    [Theory]
    [MemberData(nameof(EqualValueObjects))]
    public void NonEqualOperator_ShouldReturnFalse_WhenEqualValueObjects(ValueObject instanceA, ValueObject instanceB, string reason)
    {
        // Act
        var sut = instanceA != instanceB;

        // Assert
        sut.Should().BeFalse(reason);
    }

    [Theory]
    [MemberData(nameof(EqualValueObjects))]
    public void GetHashCode_ShouldReturnSameHashCodes_WhenEqualValueObjects(ValueObject instanceA, ValueObject instanceB, string reason)
    {
        // Act
        var hashCodeA = instanceA.GetHashCode();
        var hashCodeB = instanceB.GetHashCode();

        // Assert
        hashCodeA.Should().Be(hashCodeB, reason);
    }

    [Theory]
    [MemberData(nameof(NonEqualValueObjects))]
    public void Equals_ShouldReturnFalse_WhenNonEqualValueObjects(ValueObject instanceA, ValueObject instanceB, string reason)
    {
        // Act
        var sut = instanceA.Equals(instanceB);

        // Assert
        sut.Should().BeFalse(reason);
    }

    [Theory]
    [MemberData(nameof(NonEqualValueObjects))]
    public void EqualOperator_ShouldReturnFalse_WhenNonEqualValueObjects(ValueObject instanceA, ValueObject instanceB, string reason)
    {
        // Act
        var sut = instanceA == instanceB;

        // Assert
        sut.Should().BeFalse(reason);
    }

    [Theory]
    [MemberData(nameof(NonEqualValueObjects))]
    public void NonEqualOperator_ShouldReturnTrue_WhenNonEqualValueObjects(ValueObject instanceA, ValueObject instanceB, string reason)
    {
        // Act
        var sut = instanceA != instanceB;

        // Assert
        sut.Should().BeTrue(reason);
    }

    [Theory]
    [MemberData(nameof(NonEqualValueObjects))]
    public void GetHashCode_ShouldReturnDifferentHashCodes_WhenNonEqualValueObjects(ValueObject instanceA, ValueObject instanceB, string reason)
    {
        // Act
        var hashCodeA = instanceA.GetHashCode();
        var hashCodeB = instanceB.GetHashCode();

        // Assert
        hashCodeA.Should().NotBe(hashCodeB, reason);
    }

    public static readonly TheoryData<ValueObject, ValueObject, string> EqualValueObjects = new()
    {
        {
            APrettyValueObject,
            APrettyValueObject,
            "they should be equal because they are the same object"
        },
        {
            new ValueObjectA(1, "2", Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0")),
            new ValueObjectA(1, "2", Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0")),
            "they should be equal because they have equal members"
        },
        {
            new ValueObjectA(1, "2", Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0"),
                "alpha"),
            new ValueObjectA(1, "2", Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0"),
                "beta"),
            "they should be equal because all equality components are equal, even though an additional member was set"
        },
        {
            new ValueObjectB(1, "2", 1, 2, 3),
            new ValueObjectB(1, "2", 1, 2, 3),
            "they should be equal because all equality components are equal, including the 'C' list"
        }
    };

    public static readonly TheoryData<ValueObject?, ValueObject?, string> NonEqualValueObjects = new()
    {
        {
            new ValueObjectA(1, "2", Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0")),
            new ValueObjectA(2, "2", Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0")),
            "they should not be equal because the 'A' member on ValueObjectA is different among them"
        },
        {
            new ValueObjectA(1, "2", Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0")),
            new ValueObjectA(1, null, Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0")),
            "they should not be equal because the 'B' member on ValueObjectA is different among them"
        },
        {
            new ValueObjectA(1, "2", Guid.Parse("97ea43f0-6fef-4fb7-8c67-9114a7ff6ec0")),
            new ValueObjectB(1, "2"),
            "they should not be equal because they are not of the same type"
        },
        {
            new ValueObjectB(1, "2", 1, 2, 3),
            new ValueObjectB(1, "2", 1, 2, 3, 4),
            "they should be not be equal because the 'C' list contains one additional value"
        },
        {
            new ValueObjectB(1, "2", 1, 2, 3, 5),
            new ValueObjectB(1, "2", 1, 2, 3),
            "they should be not be equal because the 'C' list contains one additional value"
        },
        {
            new ValueObjectB(1, "2", 1, 2, 3, 5),
            new ValueObjectB(1, "2", 1, 2, 3, 4),
            "they should be not be equal because the 'C' lists are not equal"
        }
    };

    private class ValueObjectA : ValueObject
    {
        public ValueObjectA(int a, string? b, Guid c, string? notAnEqualityComponent = null)
        {
            this.A = a;
            this.B = b;
            this.C = c;
            this.NotAnEqualityComponent = notAnEqualityComponent;
        }

        private int A { get; }
        private string? B { get; }
        private Guid C { get; }

        // ReSharper disable once UnusedAutoPropertyAccessor.Local
        private string? NotAnEqualityComponent { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.A;
            yield return this.B;
            yield return this.C;
        }
    }

    private class ValueObjectB : ValueObject
    {
        public ValueObjectB(int a, string? b, params int[] c)
        {
            this.A = a;
            this.B = b;
            this.C = c.ToList();
        }

        private int A { get; }
        private string? B { get; }
        private List<int> C { get; }

        protected override IEnumerable<object?> GetEqualityComponents()
        {
            yield return this.A;
            yield return this.B;

            foreach (var c in this.C) yield return c;
        }
    }
}
