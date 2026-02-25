# Mjolnir

Mjolnir is a collection of .NET extensions and libraries designed to simplify common development tasks in ASP.NET Core and beyond.

## Project Structure

- `Mjolnir.Extensions.AspNetCore.Filtering`: A library providing easy-to-use filtering and sorting extensions for `IEnumerable<T>` and `IQueryable<T>` using attributes.
- `Mjolnir.Extensions.Railway`: (TODO: Document purpose) Currently under development.
- `Mjolnir.Extensions.AspNetCore.FilteringTest`: Unit tests and usage examples for the filtering library.
- `scripts/`: Useful scripts for building and packaging.

## Requirements

- .NET 10.0 SDK or later

## Setup and Build

To build the entire solution:

```bash
dotnet build
```

To run tests:

```bash
dotnet test
```

## Scripts

### Packaging

The `scripts/packageFiltering.sh` script can be used to create a NuGet package for the filtering library.

```bash
./scripts/packageFiltering.sh [output_directory] [version]
```

- `output_directory`: (Optional) Defaults to `$HOME/NuGetPackages/`.
- `version`: (Optional) Defaults to `1.0.0`.

## License

This project is licensed under the terms of the license found in the [LICENSE](LICENSE) file.
