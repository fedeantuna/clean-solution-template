using CleanSolutionTemplate.Application.Common.Specifications;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using CleanSolutionTemplate.Domain.Common;
using FakeItEasy;
using FluentAssertions;

namespace CleanSolutionTemplate.Application.Tests.Unit.Common.Specifications;

public class SpecificationTests
{
    public static IEnumerable<object[]> AtLeastOneFalseSpecificationData =>
        new List<object[]>
        {
            new object[]
            {
                new FalseSpecificationFake(),
                new TrueSpecificationFake()
            },
            new object[]
            {
                new TrueSpecificationFake(),
                new FalseSpecificationFake()
            },
            new object[]
            {
                new FalseSpecificationFake(),
                new FalseSpecificationFake()
            }
        };

    public static IEnumerable<object[]> AtLeastOneTrueSpecificationData =>
        new List<object[]>
        {
            new object[]
            {
                new FalseSpecificationFake(),
                new TrueSpecificationFake()
            },
            new object[]
            {
                new TrueSpecificationFake(),
                new FalseSpecificationFake()
            },
            new object[]
            {
                new TrueSpecificationFake(),
                new TrueSpecificationFake()
            }
        };

    public static IEnumerable<object[]> AtLeastOneAllSpecificationData =>
        new List<object[]>
        {
            new object[]
            {
                Specification<Entity>.All,
                new TrueSpecificationFake()
            },
            new object[]
            {
                Specification<Entity>.All,
                new FalseSpecificationFake()
            },
            new object[]
            {
                new TrueSpecificationFake(),
                Specification<Entity>.All
            },
            new object[]
            {
                new FalseSpecificationFake(),
                Specification<Entity>.All
            },
            new object[]
            {
                Specification<Entity>.All,
                Specification<Entity>.All
            }
        };

    public static IEnumerable<object[]> UnarySpecifications =>
        new List<object[]>
        {
            new object[]
            {
                new TrueSpecificationFake()
            },
            new object[]
            {
                new FalseSpecificationFake()
            }
        };

    [Fact]
    public void All_ShouldAlwaysReturnTrue_WhenEvaluatingAnEntity()
    {
        // Arrange
        var allSpecification = Specification<Entity>.All;

        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            Guid.NewGuid()
        }));

        // Act
        var result = allSpecification.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void And_ShouldReturnLeftSpecification_WhenRightSpecificationIsAll()
    {
        // Arrange
        var leftSpecification = A.Fake<Specification<Entity>>();
        var rightSpecification = Specification<Entity>.All;

        // Act
        var result = leftSpecification.And(rightSpecification);

        // Assert
        result.Should().Be(leftSpecification);
    }

    [Fact]
    public void And_ShouldReturnRightSpecification_WhenLeftSpecificationIsAll()
    {
        // Arrange
        var leftSpecification = Specification<Entity>.All;
        var rightSpecification = A.Fake<Specification<Entity>>();

        // Act
        var result = leftSpecification.And(rightSpecification);

        // Assert
        result.Should().Be(rightSpecification);
    }

    [Theory]
    [MemberData(nameof(AtLeastOneFalseSpecificationData))]
    public void IsSatisfiedBy_ShouldReturnFalse_WhenAtLeastOneSpecificationCombinedByAndIsFalse(Specification<Entity> leftSpecification,
        Specification<Entity> rightSpecification)
    {
        // Arrange
        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            Guid.NewGuid()
        }));

        // Act
        var specification = leftSpecification.And(rightSpecification);
        var result = specification.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public void IsSatisfiedBy_ShouldReturnTrue_WhenBothSpecificationsCombinedByAndAreTrue()
    {
        // Arrange
        var leftSpecification = new TrueSpecificationFake();
        var rightSpecification = new TrueSpecificationFake();

        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            Guid.NewGuid()
        }));

        // Act
        var specification = leftSpecification.And(rightSpecification);
        var result = specification.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Theory]
    [MemberData(nameof(AtLeastOneAllSpecificationData))]
    public void Or_ShouldReturnAll_WhenAtLeastOneSpecificationIsAll(Specification<Entity> leftSpecification,
        Specification<Entity> rightSpecification)
    {
        // Arrange
        var allSpecification = Specification<Entity>.All;

        // Act
        var result = leftSpecification.Or(rightSpecification);

        // Assert
        result.Should().Be(allSpecification);
    }

    [Theory]
    [MemberData(nameof(AtLeastOneTrueSpecificationData))]
    public void IsSatisfiedBy_ShouldReturnTrue_WhenOneSpecificationCombinedByOrIsTrue(Specification<Entity> leftSpecification,
        Specification<Entity> rightSpecification)
    {
        // Arrange
        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            Guid.NewGuid()
        }));

        // Act
        var specification = leftSpecification.Or(rightSpecification);
        var result = specification.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void IsSatisfiedBy_ShouldReturnFalse_WhenBothSpecificationsCombinedByOrAreFalse()
    {
        // Arrange
        var leftSpecification = new FalseSpecificationFake();
        var rightSpecification = new FalseSpecificationFake();

        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            Guid.NewGuid()
        }));

        // Act
        var specification = leftSpecification.Or(rightSpecification);
        var result = specification.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [MemberData(nameof(UnarySpecifications))]
    public void IsSatisfiedBy_ShouldReturnTheOppositeToTheSpecification_WhenNotIsApplied(Specification<Entity> specification)
    {
        // Arrange
        var entity = A.Fake<Entity>(builder => builder.WithArgumentsForConstructor(new List<object?>
        {
            Guid.NewGuid()
        }));

        var specificationResult = specification.IsSatisfiedBy(entity);

        // Act
        var notSpecification = specification.Not();
        var result = notSpecification.IsSatisfiedBy(entity);

        // Assert
        result.Should().Be(!specificationResult);
    }
}
