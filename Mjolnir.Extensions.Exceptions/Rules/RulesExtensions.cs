using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Mjolnir.Extensions.Exceptions.Rules;

/// <summary>
///     Extension methods for adding custom validation rules to endpoint routes.
/// </summary>
public static class RulesExtensions
{
    extension(RouteHandlerBuilder builder)
    {
        /// <summary>
        ///     Adds custom validation rules to the endpoint that are executed via an endpoint filter.
        ///     Rules are applied after standard data annotation validation.
        /// </summary>
        /// <typeparam name="T">The type of model being validated.</typeparam>
        /// <param name="rulesBuilder">An action that configures the custom validation rules.</param>
        /// <returns>The route handler builder for chaining.</returns>
        public RouteHandlerBuilder AddRules<T>(Action<RuleSet<T>> rulesBuilder) => builder
            .ProducesValidationProblem()
            .AddEndpointFilter(async (context, next) =>
            {
                T? model = context.Arguments.OfType<T>().FirstOrDefault();
                if (model is null) return await next(context);

                ValidationContext validationContext = new(model);

                List<ValidationResult> results = [];

                bool isValid = Validator.TryValidateObject(model, validationContext, results, true);

                RuleSet<T> ruleSet = new();
                rulesBuilder(ruleSet);
                isValid &= await ruleSet.RunRules(model, context.HttpContext, results);

                Dictionary<string, string[]> errors = results
                    .SelectMany(r =>
                    {
                        string[] members = r.MemberNames.ToArray();
                        return members.Length == 0 ?
                            [r] :
                            members.Select(m => new ValidationResult(r.ErrorMessage, [m]));
                    })
                    .GroupBy(r => r.MemberNames.FirstOrDefault() ?? "Error")
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(x => x.ErrorMessage ?? "Invalid value").ToArray()
                    );

                return isValid ? await next(context) : Results.ValidationProblem(errors);
            });
    }
}
