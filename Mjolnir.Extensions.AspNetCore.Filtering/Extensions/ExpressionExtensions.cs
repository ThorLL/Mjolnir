using System.Linq.Expressions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

internal static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AsOrExpression<T>(this IEnumerable<Expression<Func<T, bool>>> expressions)
    {
        if (!expressions.TryGetNonEnumeratedCount(out int count))
        {
            expressions = expressions.ToArray();
            count = expressions.Count();
        }

        if (count == 0) return _ => true;
        if (count == 1) return expressions.Single();

        ParameterExpression parameter = Expression.Parameter(typeof(T), "x");
        ParameterReplacerVisitor visitor = new(parameter);

        IEnumerable<Expression> bodies = expressions.Select(expr => visitor.Visit(expr.Body));
        Expression combinedBody = bodies.Aggregate(Expression.OrElse);

        return Expression.Lambda<Func<T, bool>>(combinedBody, parameter);
    }

    private class ParameterReplacerVisitor(ParameterExpression parameter) : ExpressionVisitor
    {
        protected override Expression VisitParameter(ParameterExpression node) => parameter;
    }
}
