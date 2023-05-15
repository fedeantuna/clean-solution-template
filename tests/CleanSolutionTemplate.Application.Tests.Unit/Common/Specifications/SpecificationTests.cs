using CleanSolutionTemplate.Application.Common.Specifications;
using CleanSolutionTemplate.Application.Tests.Unit.Fakes;
using CleanSolutionTemplate.Domain.Common;
using FluentAssertions;
using Moq;

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

        var entity = new Mock<Entity>(Guid.NewGuid()).Object;

        // Act
        var result = allSpecification.IsSatisfiedBy(entity);

        // Assert
        result.Should().BeTrue();
    }

    [Fact]
    public void And_ShouldReturnLeftSpecification_WhenRightSpecificationIsAll()
    {
        // Arrange
        var leftSpecification = new Mock<Specification<Entity>>().Object;
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
        var rightSpecification = new Mock<Specification<Entity>>().Object;

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
        var entity = new Mock<Entity>(Guid.NewGuid()).Object;

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

        var entity = new Mock<Entity>(Guid.NewGuid()).Object;

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
        var entity = new Mock<Entity>(Guid.NewGuid()).Object;

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

        var entity = new Mock<Entity>(Guid.NewGuid()).Object;

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
        var entity = new Mock<Entity>(Guid.NewGuid()).Object;

        var specificationResult = specification.IsSatisfiedBy(entity);

        // Act
        var notSpecification = specification.Not();
        var result = notSpecification.IsSatisfiedBy(entity);

        // Assert
        result.Should().Be(!specificationResult);
    }

    [Fact]
    public void IsSatisfiedBy_ShouldThrowArgumentException_WhenRightAndSpecificationIsNull()
    {
        // Arrange
        var leftSpecification = new Mock<Specification<Entity>>().Object;
        var rightSpecification = (Specification<Entity>)null!;

        var specification = leftSpecification.And(rightSpecification);

        // Act
        var act = () => specification.IsSatisfiedBy(new Mock<Entity>().Object);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void IsSatisfiedBy_ShouldThrowArgumentException_WhenRightOrSpecificationIsNull()
    {
        // Arrange
        var leftSpecification = new Mock<Specification<Entity>>().Object;
        var rightSpecification = (Specification<Entity>)null!;

        var specification = leftSpecification.Or(rightSpecification);

        // Act
        var act = () => specification.IsSatisfiedBy(new Mock<Entity>().Object);

        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
