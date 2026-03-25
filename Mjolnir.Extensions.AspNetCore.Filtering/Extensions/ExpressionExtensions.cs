using System.Linq.Expressions;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

/// <summary>
///     Internal extension methods for combining filter expressions using OR logic.
/// </summary>
internal static class ExpressionExtensions
{
    /// <summary>
    ///     Combines multiple filter expressions using OR logic (disjunction).
    ///     If no expressions are provided, returns an expression that always returns true.
    ///     If a single expression is provided, returns that expression as-is.
    /// </summary>
    /// <typeparam name="T">The type being filtered.</typeparam>
    /// <param name="expressions">The filter expressions to combine with OR logic.</param>
    /// <returns>A combined expression that evaluates to true if any of the input expressions return true.</returns>
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

    /// <summary>
    ///     Expression visitor that replaces all parameter references with a specified parameter.
    ///     Used internally to consolidate multiple expressions with different parameters into a single parameter.
    /// </summary>
    private class ParameterReplacerVisitor(ParameterExpression parameter) : ExpressionVisitor
    {
        /// <summary>
        ///     Replaces the visited parameter with the specified parameter.
        /// </summary>
        /// <param name="node">The parameter expression to replace.</param>
        /// <returns>The replacement parameter expression.</returns>
        protected override Expression VisitParameter(ParameterExpression node) => parameter;
    }
}
