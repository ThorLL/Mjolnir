using Mjolnir.Extensions.AspNetCore.Filtering.Attributes;
using Mjolnir.Extensions.AspNetCore.Filtering.Attributes.Filters;
using Mjolnir.Extensions.AspNetCore.FilteringTest.Attributes;

namespace Mjolnir.Extensions.AspNetCore.FilteringTest.Models;

public class TestProduct
{
    public int Id { get; set; }

    [Sortable, StringFilter] public string Name { get; set; } = string.Empty;
    [Sortable, RangeFilter<decimal>] public decimal Price { get; set; }
    [Sortable, RangeFilter<int>] public int Stock { get; set; }
    [Sortable, EnumFilter<ProductCategory>] public ProductCategory Category { get; set; }
    [BoolFilter] public bool IsActive { get; set; }
    [NullFilter] public string? Description { get; set; }

    // Custom filter test field: use custom attribute working on this property name
    [CustomStartsWithFilter] public string NameAlias { get; set; } = string.Empty;

    // Custom sort test field: direction in valueString, sorts by Name length
    [CustomNameLengthSort] public string DummySort { get; set; } = string.Empty;

    [Sortable] public DateTime CreatedAt { get; set; }
}

public enum ProductCategory { Electronics, Clothing, Books, Home, Sports }
