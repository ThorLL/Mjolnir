# Mjolnir.Extensions.Railway

A lightweight Railway Oriented Programming (ROP) library for C# that provides a `Result<TSuccess, TFailure>` type for elegant error handling.

## Installation

```bash
dotnet add package Mjolnir.Extensions.Railway
```

## Quick Start

```csharp
using Mjolnir.Extensions.Railway;

// Create success/failure results
var success = Result.Success<int, Exception>(42);
var failure = Result.Failure<int, Exception>(new InvalidOperationException("Something went wrong"));

// Safely execute code that might throw (defaults to Result<T, Exception>)
var result = Result.RunCatching(() => int.Parse("123"));
```

## Core API

```csharp
/// --- Creating Results ---

Result.Success<TSuccess, TFailure>(value)       // Wrap a value as success
Result.Failure<TSuccess, TFailure>(failure)     // Wrap a failure value
Result.RunCatching(() => ...)                   // Execute and catch any exceptions (returns Result<T, Exception>)

/// --- Checking State ---
result.IsSuccess                                // true if success
result.IsFailure                                // true if failure

/// --- Extracting Values ---
result.TryGetSuccess(out var success)           // Safely try to get success value
result.TryGetFailure(out var failure)           // Safely try to get failure value
result.Unfold(out var success, out var failure) // Safely try unfold success and failure values (returns true on success)

/// --- Transformations ---
result.Map(x => x * 2)                          // Transform success value (keeps same TFailure)
result.MapCatching(x => ...)                    // Transform and catch any exceptions (returns Result<TResult, Exception>)
result.Recover(failure => fallbackValue)        // Recover from failure (where TSuccess : TRecover)
result.RecoverCatching(failure => ...)          // Recover and catch exceptions (returns Result<TRecover, Exception>)

/// --- Side Effects ---
result.OnSuccess(x => Console.WriteLine(x))     // Execute on success
result.OnFailure(f => Log(f))                   // Execute on failure

/// --- Folding ---
string message = result.Fold(
    onSuccess: value => $"Got {value}",
    onFailure: failure => $"Error: {failure}"
)
```

## Example: Chaining Operations

```csharp
string status = Result.RunCatching(() => GetUserInput())
    .Map(input => int.Parse(input))
    .Map(num => num * 2)
    .OnSuccess(x => Console.WriteLine($"Result: {x}"))
    .OnFailure(ex => Console.WriteLine($"Error: {ex.Message}"))
    .Fold(
        onSuccess: value => "Processed successfully!",
        onFailure: ex => $"Failed: {ex.Message}"
    );
```

## License
MIT [License](../LICENSE).
