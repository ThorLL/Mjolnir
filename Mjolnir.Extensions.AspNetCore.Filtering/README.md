# Mjolnir.Extensions.AspNetCore.Filtering

A library for simple entity filtering and sorting in ASP.NET Core projects. It provides extensions for `IEnumerable<T>` and `IQueryable<T>` that utilize attributes on property members to define filtering logic.

## Features

- **Attribute-Based Filtering**: Mark your model properties with attributes like `[StringFilter]`, `[RangeFilter<T>]`, `[EnumFilter<T>]`, `[BoolFilter]`, and `[NullFilter]`.
- **Sorting Support**: Use the `[Sortable]` attribute to enable sorting on specific properties.
- **Custom Filters and Sorting**: Extend functionality by implementing your own attributes through `[CustomFilter<T>]` or `[CustomSorting<T>]`.
- **Works with `IEnumerable<T>` and `IQueryable<T>`**: Effortlessly filter and sort in-memory collections or database queries.

## Installation

Add the library to your project:

```bash
dotnet add package Mjolnir.Extensions.AspNetCore.Filtering
```

## Usage

### Define Your Model

Apply filtering and sorting attributes to your entity or DTO:

```csharp
public class Product
{
    public int Id { get; set; }

    [Sortable, StringFilter]
    public string Name { get; set; } = string.Empty;

    [Sortable, RangeFilter<decimal>]
    public decimal Price { get; set; }

    [Sortable, EnumFilter<Category>]
    public Category Category { get; set; }

    [BoolFilter]
    public bool IsActive { get; set; }

    [NullFilter]
    public string? Description { get; set; }
}

public enum Category { Electronics, Clothing, Books }
```

### Apply Filtering and Sorting

Use the `FilterBy` and `SortBy` extension methods:

```csharp
using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

// Example collections
IEnumerable<Product> products = GetProducts();

// Filter by name (OR logic)
var result = products.FilterBy("Name:Laptop|Book");

// Filter by price range (Min-Max)
result = products.FilterBy("Price:10-100");

// Filter by price range (Unbounded)
result = products.FilterBy("Price:10-*"); // Greater than or equal to 10

// Multiple filters (AND logic)
result = products.FilterBy("Name:Laptop,Price:900-1100");

// Sorting
result = products.SortBy("Price:Asc");
result = products.SortBy("Name:Desc");
```

## Syntax Patterns

| Type                | Pattern                  | Description                                    |
|:--------------------|:-------------------------|:-----------------------------------------------|
| **Filter**          | `Prop:Value`             | Simple match                                   |
| **OR Filter**       | `Prop:Val1\|Val2`        | Matches Val1 OR Val2                           |
| **Range Filter**    | `Prop:Min-Max`           | Matches values between Min and Max (inclusive) |
| **Unbounded Range** | `Prop:0-*`               | Matches values greater than or equal to 0      |
| **Sorting**         | `Prop:Asc` / `Prop:Desc` | Sorts property in given direction              |
| **Multiple**        | `P1:V1,P2:V2`            | Applies both filters (AND logic)               |

## Tests

To run the unit tests:

```bash
dotnet test Mjolnir.Extensions.AspNetCore.FilteringTest
```

## License

MIT (See root [LICENSE](../../LICENSE) for details).
