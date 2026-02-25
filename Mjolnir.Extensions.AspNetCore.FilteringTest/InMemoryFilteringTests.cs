using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;
using Mjolnir.Extensions.AspNetCore.FilteringTest.Models;

namespace Mjolnir.Extensions.AspNetCore.FilteringTest;

public class InMemoryFilteringTests
{
    private readonly List<TestProduct> _products = new()
    {
        new TestProduct
        {
            Id = 1, Name = "Laptop", Price = 999.99m, Stock = 10, Category = ProductCategory.Electronics,
            CreatedAt = DateTime.Now.AddDays(-5)
        },
        new TestProduct
        {
            Id = 2, Name = "T-Shirt", Price = 19.99m, Stock = 50, Category = ProductCategory.Clothing,
            CreatedAt = DateTime.Now.AddDays(-3)
        },
        new TestProduct
        {
            Id = 3, Name = "Book", Price = 12.50m, Stock = 25, Category = ProductCategory.Books,
            CreatedAt = DateTime.Now.AddDays(-1)
        },
        new TestProduct
        {
            Id = 4, Name = "Chair", Price = 149.99m, Stock = 8, Category = ProductCategory.Home,
            CreatedAt = DateTime.Now.AddDays(-7)
        },
        new TestProduct
        {
            Id = 5, Name = "Basketball", Price = 29.99m, Stock = 15, Category = ProductCategory.Sports,
            CreatedAt = DateTime.Now.AddDays(-2)
        },
        new TestProduct
        {
            Id = 6, Name = "Smartphone", Price = 599.99m, Stock = 20, Category = ProductCategory.Electronics,
            CreatedAt = DateTime.Now.AddDays(-4)
        },
        new TestProduct
        {
            Id = 7, Name = "Jeans", Price = 79.99m, Stock = 30, Category = ProductCategory.Clothing,
            CreatedAt = DateTime.Now.AddDays(-6)
        }
    };

    [Fact]
    public void FilterBy_IEnumerable_StringFilter_ShouldWork()
    {
        // Act
        List<TestProduct> result = _products.FilterBy("Name:Laptop").ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Laptop", result[0].Name);
    }

    [Fact]
    public void FilterBy_IEnumerable_StringFilter_WithOr_ShouldWork()
    {
        // Act
        List<TestProduct> result = _products.FilterBy("Name:Laptop|Book").ToList();

        // Assert
        Assert.Equal(2, result.Count);
        List<string> names = result.Select(p => p.Name).ToList();
        Assert.Contains("Laptop", names);
        Assert.Contains("Book", names);
    }

    [Fact]
    public void FilterBy_IEnumerable_EnumFilter_WithOr_ShouldWork()
    {
        // Act
        List<TestProduct> result = _products.FilterBy("Category:Electronics|Clothing").ToList();

        // Assert
        Assert.Equal(4, result.Count); // Laptop, Smartphone, T-Shirt, Jeans
        Assert.All(result,
            p => Assert.True(p.Category == ProductCategory.Electronics || p.Category == ProductCategory.Clothing));
    }

    [Fact]
    public void FilterBy_IEnumerable_RangeFilter_ShouldWork()
    {
        // Act
        List<TestProduct> result = _products.FilterBy("Price:10-30").ToList();

        // Assert
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.True(p.Price is >= 10 and <= 30));
    }

    [Fact]
    public void SortBy_IEnumerable_ShouldWork()
    {
        // Act
        List<TestProduct> result = _products.SortBy("Price:Asc").ToList();

        // Assert
        Assert.Equal(7, result.Count);
        Assert.Equal("Book", result[0].Name); // Cheapest
        Assert.Equal("Laptop", result[6].Name); // Most expensive
    }
}
