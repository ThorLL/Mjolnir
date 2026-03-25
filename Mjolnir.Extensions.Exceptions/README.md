# Mjolnir.Extensions.Exceptions

Simple MCP server compatible exceptions, HTTP request validators, and global handlers for ASP.NET Core.

## Overview

Mjolnir.Extensions.Exceptions provides a standardized way to handle exceptions and validation in ASP.NET Core applications. It includes base exception classes, global exception handlers that convert exceptions to `ProblemDetails`, and a fluent validation rule system for Minimal APIs.

### Key Features:
- **MjolnirException**: A base exception class with extensive static helper methods for common validation checks (e.g., `ThrowIfNull`, `ThrowIfZero`, `ThrowIfNegative`).
- **ValidationException**: A specialized exception for accumulating and reporting multiple validation errors.
- **Global Handlers**: Middleware handlers for `MjolnirException` and `ValidationException` that automatically produce standardized `ProblemDetails` responses.
- **RuleSet & Minimal API Integration**: A fluent API for defining custom validation rules on endpoints.

## Requirements

- **Runtime**: .NET 10.0+
- **Language**: C# 14.0+
- **Dependencies**:
  - `Microsoft.AspNetCore.OpenApi` (10.0.5)
  - `Microsoft.Extensions.DependencyInjection.Abstractions` (10.0.5)
  - `ModelContextProtocol.Core` (1.1.0)

## Setup

### Installation

Install the package via NuGet:

```bash
dotnet add package Mjolnir.Extensions.Exceptions
```

### Configuration

Register the exception handlers in your `Program.cs`:

```csharp
using Mjolnir.Extensions.Exceptions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

// Add Mjolnir exception handlers
builder.Services.AddMjolnirExceptionsHandler();

var app = builder.Build();

// Use the exception handler middleware
app.UseMjolnirExceptionHandler();

app.Run();
```

## Usage

### MjolnirException

Use the static helper methods to perform validation and throw exceptions with specific HTTP status codes:

```csharp
MjolnirException.ThrowIfNull(user, "User not found", HttpStatusCode.NotFound);
MjolnirException.ThrowIfNegative(price, "Price cannot be negative");
```

### ValidationException

Accumulate multiple validation errors:

```csharp
var ex = new ValidationException();

if (string.IsNullOrEmpty(request.Name))
    ex.WithError(nameof(request.Name), "Name is required");

if (request.Age < 18)
    ex.WithError(nameof(request.Age), "Must be at least 18");

ex.ThrowIfHasError();
```

### Minimal API RuleSets

Define custom validation rules for your endpoints:

```csharp
app.MapPost("/users", (User user) => Results.Ok(user))
   .AddRules<User>(rules =>
   {
       rules.AddRule(u => !string.IsNullOrEmpty(u.Username), "Username is required", nameof(User.Username));
       rules.AddRule(async u => await IsUsernameUnique(u.Username), "Username already taken", nameof(User.Username));
   });
```

## Project Structure

```text
Mjolnir.Extensions.Exceptions/
├── DependencyInjection/
│   └── Di.cs                       # DI registration extensions
├── Handlers/
│   ├── MjolnirExceptionHandler.cs   # Global handler for MjolnirException
│   └── ValidationExceptionHandler.cs # Global handler for ValidationException
├── Rules/
│   ├── RuleSet.cs                   # Fluent validation rule builder
│   └── RulesExtensions.cs           # Endpoint filter extensions for Rules
├── MjolnirException.cs              # Base exception with helper methods
├── ValidationException.cs           # Multi-error validation exception
└── Mjolnir.Extensions.Exceptions.csproj
```

## License

This project is licensed under the [LICENSE](../LICENSE) file in the root of the repository.
