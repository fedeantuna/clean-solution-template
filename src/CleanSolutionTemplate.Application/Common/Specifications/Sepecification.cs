using System.Linq.Expressions;
using CleanSolutionTemplate.Domain.Common;

namespace CleanSolutionTemplate.Application.Common.Specifications;

public abstract class Specification<TEntity>
    where TEntity : Entity
{
    public static readonly Specification<TEntity> All = new IdentitySpecification<TEntity>();

    public bool IsSatisfiedBy(TEntity entity) =>
        this.ToExpression().Compile()(entity);

    public abstract Expression<Func<TEntity, bool>> ToExpression();

    public Specification<TEntity> And(Specification<TEntity> specification)
    {
        if (this == All)
        {
            return specification;
        }
        if (specification == All)
        {
            return this;
        }

        return new AndSpecification<TEntity>(this, specification);
    }

    public Specification<TEntity> Or(Specification<TEntity> specification)
    {
        if (this == All || specification == All)
        {
            return All;
        }

        return new OrSpecification<TEntity>(this, specification);
    }

    public Specification<TEntity> Not()
        => new NotSpecification<TEntity>(this);

    private sealed class IdentitySpecification<T> : Specification<T>
        where T : Entity
    {
        public override Expression<Func<T, bool>> ToExpression() =>
            x => true;
    }

    private sealed class AndSpecification<T> : Specification<T>
        where T : Entity
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public AndSpecification(Specification<T> left, Specification<T> right)
        {
            this._left = left;
            this._right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = this._left.ToExpression();
            var rightExpression = this._right.ToExpression();

            var andExpression = Expression.AndAlso(leftExpression.Body, rightExpression.Body);

            return Expression.Lambda<Func<T, bool>>(andExpression, leftExpression.Parameters.Single());
        }
    }

    private sealed class OrSpecification<T> : Specification<T>
        where T : Entity
    {
        private readonly Specification<T> _left;
        private readonly Specification<T> _right;

        public OrSpecification(Specification<T> left, Specification<T> right)
        {
            this._left = left;
            this._right = right;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var leftExpression = this._left.ToExpression();
            var rightExpression = this._right.ToExpression();

            var andExpression = Expression.OrElse(leftExpression.Body, rightExpression.Body);

            return Expression.Lambda<Func<T, bool>>(andExpression, leftExpression.Parameters.Single());
        }
    }

    private sealed class NotSpecification<T> : Specification<T>
        where T : Entity
    {
        private readonly Specification<T> _specification;

        public NotSpecification(Specification<T> specification)
        {
            this._specification = specification;
        }

        public override Expression<Func<T, bool>> ToExpression()
        {
            var expression = this._specification.ToExpression();

            var notExpression = Expression.Not(expression.Body);

            return Expression.Lambda<Func<T, bool>>(notExpression, expression.Parameters.Single());
        }
    }
}
