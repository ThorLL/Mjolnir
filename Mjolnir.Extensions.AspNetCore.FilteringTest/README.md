# Mjolnir.Extensions.AspNetCore.Filtering Tests

This project contains comprehensive tests for the filtering and sorting functionality.

## Test Coverage

### InMemoryFilteringTests
- Tests filtering and sorting on in-memory collections (IEnumerable)
- Tests GetFilterOptions functionality
- Tests GetSortableProps functionality
- Tests OR filtering with pipe symbol

### EfInMemoryFilteringTests  
- Tests filtering and sorting with Entity Framework using InMemory provider
- Validates that expressions translate correctly to database queries
- Tests string, range, and enum filtering
- Tests OR filtering with pipe symbol for string and enum filters
- Tests single and multi-property sorting
- Tests combined filtering and sorting
- Tests error handling for invalid inputs

## Filtering Features

### Basic Filtering
- **String filters**: `Name:Laptop` (contains search)
- **Range filters**: `Price:10-50`, `Price:*-50`, `Price:10-*` (wildcards supported)
- **Enum filters**: `Category:Electronics`

### OR Filtering (NEW)
Use the pipe symbol `|` to create OR conditions within a single property:
- **String OR**: `Name:Laptop|Book` (matches items containing "Laptop" OR "Book")
- **Enum OR**: `Category:Electronics|Clothing` (matches Electronics OR Clothing categories)
- **Spaces handled**: `Category: Electronics | Clothing ` (automatically trims spaces)

### Combined Filtering
Combine multiple filters with commas (AND logic between different properties):
- `Category:Electronics,Price:*-1000` (Electronics AND under $1000)
- `Category:Electronics|Clothing,Price:*-100` (Electronics OR Clothing, AND under $100)

## Test Data Model

The tests use a `TestProduct` model with the following filterable/sortable properties:

- `Name` - String filter, sortable
- `Price` - Range filter (decimal), sortable  
- `Stock` - Range filter (int), sortable
- `Category` - Enum filter, sortable
- `CreatedAt` - Sortable only

## Running Tests

```bash
dotnet test
```

All tests should pass and demonstrate that the filtering/sorting works correctly both in-memory and with database queries.