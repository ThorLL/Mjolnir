# Mjolnir

Mjolnir is a collection of .NET extensions and libraries designed to simplify common development tasks in ASP.NET Core and beyond.

## Stack

- **Language:** C# 14.0
- **Framework:** .NET 10.0 (ASP.NET Core)
- **Package Manager:** NuGet
- **Testing:** xUnit

## Packages

### Mjolnir.Extensions.AspNetCore.Filtering
A library providing easy-to-use filtering and sorting extensions for `IEnumerable<T>` and `IQueryable<T>` using attributes.

### Mjolnir.Extensions.Railway
A lightweight Railway Oriented Programming (ROP) library providing `Result<TSuccess, TFailure>` for elegant error handling.

## Requirements

- .NET 10.0 SDK or later

## Setup and Build

### Build
Build all projects in the solution:
```bash
dotnet build
```

### Test
Run all tests in the solution:
```bash
dotnet test
```

To run tests for a specific project:
```bash
dotnet test Mjolnir.Extensions.AspNetCore.FilteringTest
```

### Pack
To create NuGet packages locally:
```bash
dotnet pack -c Release
```

## Packaging and Release

This project uses GitHub Actions to automate the packaging process.

### Automated Releases

A NuGet package is automatically created and uploaded as an artifact when a new GitHub Release is published. The package to be created is determined by the tag prefix:

- `filtering-*`: Packages `Mjolnir.Extensions.AspNetCore.Filtering`
- `railway-*`: Packages `Mjolnir.Extensions.Railway`

Example: Creating a release with tag `filtering-1.1.0` will package the filtering library with version `1.1.0`.

### Configuration

To allow the workflow to publish to `nuget.org`, you must:
1. Generate an API Key on [nuget.org](https://www.nuget.org/profiles/ManageAPIKeys).
2. Add it as a Repository Secret named `NUGET_API_KEY` in your GitHub repository settings (`Settings` > `Secrets and variables` > `Actions`).

## License

This project is licensed under the terms of the license found in the [LICENSE](LICENSE) file.
