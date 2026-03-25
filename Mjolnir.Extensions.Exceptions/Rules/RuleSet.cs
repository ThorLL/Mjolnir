using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Mjolnir.Extensions.Exceptions.Rules;

/// <summary>
///     A builder for defining custom validation rules that can be async or take the <see cref="HttpContext" />.
///     Rules are executed as a set and errors are accumulated.
/// </summary>
/// <typeparam name="T">The type of model being validated.</typeparam>
public class RuleSet<T>
{
    private readonly List<(Func<T, HttpContext, Task<bool>>, ValidationResult)> _rules = [];

    /// <summary>
    ///     Executes all rules in the rule set against the specified model.
    /// </summary>
    /// <param name="model">The model to validate.</param>
    /// <param name="context">The HTTP context for context-aware validation.</param>
    /// <param name="validationResults">The collection to add validation failures to.</param>
    /// <returns>
    ///     A task that returns <c>true</c> if all rules pass, <c>false</c> if any rule fails.
    /// </returns>
    internal async Task<bool> RunRules(
        T model,
        HttpContext context,
        ICollection<ValidationResult> validationResults
    )
    {
        bool isValid = true;
        foreach ((Func<T, HttpContext, Task<bool>> predicate, ValidationResult error) in _rules)
        {
            if (await predicate(model, context)) continue;
            isValid = false;
            validationResults.Add(error);
        }

        return isValid;
    }

    /// <summary>
    ///     Adds a synchronous validation rule.
    /// </summary>
    /// <param name="predicate">A function that returns <c>true</c> if validation passes, <c>false</c> if it fails.</param>
    /// <param name="errorMessage">The error message to add if validation fails.</param>
    /// <param name="memberNames">The property names associated with this validation error.</param>
    /// <returns>This rule set for fluent chaining.</returns>
    public RuleSet<T> AddRule(Func<T, bool> predicate, string errorMessage, params string[] memberNames)
    {
        _rules.Add(((v, _) => Task.FromResult(predicate(v)), new ValidationResult(errorMessage, memberNames)));
        return this;
    }

    /// <summary>
    ///     Adds an asynchronous validation rule.
    /// </summary>
    /// <param name="predicate">A function that asynchronously returns <c>true</c> if validation passes, <c>false</c> if it fails.</param>
    /// <param name="errorMessage">The error message to add if validation fails.</param>
    /// <param name="memberNames">The property names associated with this validation error.</param>
    /// <returns>This rule set for fluent chaining.</returns>
    public RuleSet<T> AddRule(Func<T, Task<bool>> predicate, string errorMessage, params string[] memberNames)
    {
        _rules.Add(((v, _) => predicate(v), new ValidationResult(errorMessage, memberNames)));
        return this;
    }

    /// <summary>
    ///     Adds a synchronous validation rule that has access to the HTTP context.
    /// </summary>
    /// <param name="predicate">A function that takes the model and HTTP context and returns <c>true</c> if validation passes.</param>
    /// <param name="errorMessage">The error message to add if validation fails.</param>
    /// <param name="memberNames">The property names associated with this validation error.</param>
    /// <returns>This rule set for fluent chaining.</returns>
    public RuleSet<T> AddRule(Func<T, HttpContext, bool> predicate, string errorMessage, params string[] memberNames)
    {
        _rules.Add(((v, c) => Task.FromResult(predicate(v, c)), new ValidationResult(errorMessage, memberNames)));
        return this;
    }

    /// <summary>
    ///     Adds an asynchronous validation rule that has access to the HTTP context.
    /// </summary>
    /// <param name="predicate">A function that asynchronously takes the model and HTTP context and returns <c>true</c> if validation passes.</param>
    /// <param name="errorMessage">The error message to add if validation fails.</param>
    /// <param name="memberNames">The property names associated with this validation error.</param>
    /// <returns>This rule set for fluent chaining.</returns>
    public RuleSet<T> AddRule(
        Func<T, HttpContext, Task<bool>> predicate,
        string errorMessage,
        params string[] memberNames
    )
    {
        _rules.Add((predicate, new ValidationResult(errorMessage, memberNames)));
        return this;
    }
}
