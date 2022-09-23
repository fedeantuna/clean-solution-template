using System.Linq.Expressions;
using CleanSolutionTemplate.Application.Common.Specifications;
using CleanSolutionTemplate.Domain.Common;

namespace CleanSolutionTemplate.Application.Tests.Unit.Fakes;

public class FakeTrueSpecification : Specification<Entity>
{
    public override Expression<Func<Entity, bool>> ToExpression() =>
        x => true;
}

public class FakeFalseSpecification : Specification<Entity>
{
    public override Expression<Func<Entity, bool>> ToExpression() =>
        x => false;
}
