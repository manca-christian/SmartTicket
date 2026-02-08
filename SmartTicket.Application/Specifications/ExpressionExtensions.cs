using System.Linq.Expressions;

namespace SmartTicket.Application.Specifications;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var param = Expression.Parameter(typeof(T), "x");

        var leftBody = ReplaceParameter(left.Body, left.Parameters[0], param);
        var rightBody = ReplaceParameter(right.Body, right.Parameters[0], param);

        return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(leftBody, rightBody), param);
    }

    private static Expression ReplaceParameter(Expression body, ParameterExpression oldParam, ParameterExpression newParam)
        => new ReplaceParameterVisitor(oldParam, newParam).Visit(body)!;

    private sealed class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _oldParam;
        private readonly ParameterExpression _newParam;

        public ReplaceParameterVisitor(ParameterExpression oldParam, ParameterExpression newParam)
        {
            _oldParam = oldParam;
            _newParam = newParam;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => node == _oldParam ? _newParam : base.VisitParameter(node);
    }
}
