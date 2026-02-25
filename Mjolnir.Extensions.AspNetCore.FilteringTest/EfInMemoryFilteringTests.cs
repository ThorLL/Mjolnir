using Microsoft.EntityFrameworkCore;
using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;
using Mjolnir.Extensions.AspNetCore.FilteringTest.Models;

namespace Mjolnir.Extensions.AspNetCore.FilteringTest;

public class TestDbContext : DbContext
{
    public TestDbContext(DbContextOptions<TestDbContext> options) : base(options)
    {
    }

    public DbSet<TestProduct> Products { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder) =>
        modelBuilder.Entity<TestProduct>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).HasMaxLength(200).IsRequired();
            entity.Property(e => e.Price).HasPrecision(18, 2);
            entity.Property(e => e.Category).HasConversion<string>();
        });
}

public class EfInMemoryFilteringTests : IDisposable
{
    private readonly TestDbContext _context;

    public EfInMemoryFilteringTests()
    {
        DbContextOptions<TestDbContext> options = new DbContextOptionsBuilder<TestDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new TestDbContext(options);

        // Seed test data
        TestProduct[] products = new[]
        {
            new TestProduct
            {
                Id = 1, Name = "Laptop", Price = 999.99m, Stock = 10, Category = ProductCategory.Electronics,
                CreatedAt = DateTime.Now.AddDays(-5), IsActive = true, Description = "High performance laptop",
                NameAlias = "Laptop", DummySort = "x"
            },
            new TestProduct
            {
                Id = 2, Name = "T-Shirt", Price = 19.99m, Stock = 50, Category = ProductCategory.Clothing,
                CreatedAt = DateTime.Now.AddDays(-3), IsActive = true, Description = null,
                NameAlias = "T-Shirt", DummySort = "x"
            },
            new TestProduct
            {
                Id = 3, Name = "Book", Price = 12.50m, Stock = 25, Category = ProductCategory.Books,
                CreatedAt = DateTime.Now.AddDays(-1), IsActive = false, Description = "A very interesting book",
                NameAlias = "Book", DummySort = "x"
            },
            new TestProduct
            {
                Id = 4, Name = "Chair", Price = 149.99m, Stock = 8, Category = ProductCategory.Home,
                CreatedAt = DateTime.Now.AddDays(-7), IsActive = true, Description = "Comfortable office chair",
                NameAlias = "Chair", DummySort = "x"
            },
            new TestProduct
            {
                Id = 5, Name = "Basketball", Price = 29.99m, Stock = 15, Category = ProductCategory.Sports,
                CreatedAt = DateTime.Now.AddDays(-2), IsActive = false, Description = null,
                NameAlias = "Basketball", DummySort = "x"
            },
            new TestProduct
            {
                Id = 6, Name = "Smartphone", Price = 599.99m, Stock = 20, Category = ProductCategory.Electronics,
                CreatedAt = DateTime.Now.AddDays(-4), IsActive = true, Description = "Latest model",
                NameAlias = "Smartphone", DummySort = "x"
            },
            new TestProduct
            {
                Id = 7, Name = "Jeans", Price = 79.99m, Stock = 30, Category = ProductCategory.Clothing,
                CreatedAt = DateTime.Now.AddDays(-6), IsActive = true, Description = "Blue jeans",
                NameAlias = "Jeans", DummySort = "x"
            }
        };

        _context.Products.AddRange(products);
        _context.SaveChanges();
    }

    public void Dispose() => _context.Dispose();

    [Fact]
    public void FilterBy_StringFilter_ShouldFilterCorrectly()
    {
        // Act
        List<TestProduct> result = _context.Products
            .FilterBy("Name:Laptop")
            .ToList();

        // Assert
        Assert.Single(result);
        Assert.Equal("Laptop", result[0].Name);
    }

    [Fact]
    public void FilterBy_RangeFilter_ShouldFilterByPriceRange()
    {
        // Act - Products between $10 and $30
        List<TestProduct> result = _context.Products
            .FilterBy("Price:10-30")
            .ToList();

        // Assert
        Assert.Equal(3, result.Count); // T-Shirt, Book, Basketball
        Assert.All(result, p => Assert.True(p.Price >= 10 && p.Price <= 30));
    }

    [Fact]
    public void FilterBy_RangeFilter_WithWildcard_ShouldUseMinValue()
    {
        // Act - Products from minimum price to $50
        List<TestProduct> result = _context.Products
            .FilterBy("Price:*-50")
            .ToList();

        // Assert - Should include T-Shirt (19.99), Book (12.50), Basketball (29.99) - Chair is 149.99 so excluded
        Assert.Equal(3, result.Count);
        Assert.All(result, p => Assert.True(p.Price <= 50));
        Assert.DoesNotContain(result, p => p.Name == "Laptop"); // 999.99 > 50
        Assert.DoesNotContain(result, p => p.Name == "Chair"); // 149.99 > 50
    }

    [Fact]
    public void FilterBy_EnumFilter_ShouldFilterByCategory()
    {
        // Act
        List<TestProduct> result = _context.Products
            .FilterBy("Category:Electronics")
            .ToList();

        // Assert
        Assert.Equal(2, result.Count); // Laptop and Smartphone
        Assert.All(result, p => Assert.Equal(ProductCategory.Electronics, p.Category));
    }

    [Fact]
    public void FilterBy_EnumFilter_WithOr_ShouldFilterByMultipleCategories()
    {
        // Act - Electronics OR Clothing
        List<TestProduct> result = _context.Products
            .FilterBy("Category:Electronics|Clothing")
            .ToList();

        // Assert
        Assert.Equal(4, result.Count); // Laptop, Smartphone, T-Shirt, Jeans
        Assert.All(result,
            p => Assert.True(p.Category == ProductCategory.Electronics || p.Category == ProductCategory.Clothing));
    }

    [Fact]
    public void FilterBy_StringFilter_WithOr_ShouldFilterByMultipleValues()
    {
        // Act - Names containing "Laptop" OR "Book"
        List<TestProduct> result = _context.Products
            .FilterBy("Name:Laptop|Book")
            .ToList();

        // Assert
        Assert.Equal(2, result.Count); // Laptop and Book
        List<string> names = result.Select(p => p.Name).ToList();
        Assert.Contains("Laptop", names);
        Assert.Contains("Book", names);
    }

    [Fact]
    public void FilterBy_MultipleFilters_ShouldApplyAllFilters()
    {
        // Act - Electronics products under $1000
        List<TestProduct> result = _context.Products
            .FilterBy("Category:Electronics,Price:*-1000")
            .ToList();

        // Assert
        Assert.Equal(2, result.Count); // Laptop and Smartphone
        Assert.All(result, p => Assert.Equal(ProductCategory.Electronics, p.Category));
    }

    [Fact]
    public void SortBy_SingleProperty_ShouldSortCorrectly()
    {
        // Act
        List<TestProduct> result = _context.Products
            .SortBy("Price:Asc")
            .ToList();

        // Assert
        Assert.Equal(7, result.Count);
        Assert.Equal("Book", result[0].Name); // Cheapest
        Assert.Equal("Laptop", result[6].Name); // Most expensive
    }

    [Fact]
    public void SortBy_MultipleProperties_ShouldSortCorrectly()
    {
        // Act - Sort by Category ascending, then Price descending
        List<TestProduct> result = _context.Products
            .SortBy("Category:Asc,Price:Desc")
            .ToList();

        // Assert
        Assert.Equal(7, result.Count);
        // Should be grouped by category, with highest price first in each group
        List<ProductCategory> categories = result.Select(p => p.Category).ToList();
        Assert.True(categories.SequenceEqual(categories.OrderBy(c => c)));
    }

    [Fact]
    public void FilterBy_AndSortBy_ShouldWorkTogether()
    {
        // Act - Filter by price range and sort by name
        List<TestProduct> result = _context.Products
            .FilterBy("Price:10-200")
            .SortBy("Name:Asc")
            .ToList();

        // Assert
        Assert.Equal(5, result.Count); // T-Shirt, Book, Basketball, Chair, Jeans
        List<string> names = result.Select(p => p.Name).ToList();
        Assert.Equal(names.OrderBy(n => n).ToList(), names);
    }

    [Theory]
    [InlineData("InvalidProperty:value")]
    [InlineData("Name:")]
    [InlineData("Price:invalid-range")]
    [InlineData("Name:|")] // Empty OR values
    [InlineData("Category:InvalidEnum")]
    public void FilterBy_InvalidInput_ShouldThrowException(string filterString) =>
        // Act & Assert
        Assert.Throws<ArgumentException>(() => { _context.Products.FilterBy(filterString).ToList(); });

    [Fact]
    public void FilterBy_StringFilter_WithOrAndSpaces_ShouldTrimAndFilter()
    {
        // Act - Test with spaces around pipe and values
        List<TestProduct> result = _context.Products
            .FilterBy("Name: Laptop | Book ")
            .ToList();

        // Assert
        Assert.Equal(2, result.Count);
        List<string> names = result.Select(p => p.Name).ToList();
        Assert.Contains("Laptop", names);
        Assert.Contains("Book", names);
    }

    [Fact]
    public void FilterBy_EnumFilter_WithOrAndSpaces_ShouldTrimAndFilter()
    {
        // Act - Test with spaces around pipe and values
        List<TestProduct> result = _context.Products
            .FilterBy("Category: Electronics | Clothing ")
            .ToList();

        // Assert
        Assert.Equal(4, result.Count);
        Assert.All(result,
            p => Assert.True(p.Category == ProductCategory.Electronics || p.Category == ProductCategory.Clothing));
    }

    [Fact]
    public void FilterBy_BoolFilter_ShouldFilterCorrectly()
    {
        // Act - Active products
        List<TestProduct> activeProducts = _context.Products
            .FilterBy("IsActive:true")
            .ToList();

        // Act - Inactive products
        List<TestProduct> inactiveProducts = _context.Products
            .FilterBy("IsActive:false")
            .ToList();

        // Assert
        Assert.Equal(5, activeProducts.Count);
        Assert.All(activeProducts, p => Assert.True(p.IsActive));

        Assert.Equal(2, inactiveProducts.Count);
        Assert.All(inactiveProducts, p => Assert.False(p.IsActive));
    }

    [Fact]
    public void FilterBy_NullFilter_ShouldFilterNotNullCorrectly()
    {
        // Act - Products with description (not null)
        List<TestProduct> productsWithDescription = _context.Products
            .FilterBy("Description:true")
            .ToList();

        // Assert
        Assert.Equal(5, productsWithDescription.Count);
        Assert.All(productsWithDescription, p => Assert.NotNull(p.Description));
    }

    [Fact]
    public void FilterBy_NullFilter_ShouldFilterNullCorrectly()
    {
        // Act - Products without description (null)
        List<TestProduct> productsWithoutDescription = _context.Products
            .FilterBy("Description:false")
            .ToList();

        // Assert
        Assert.Equal(2, productsWithoutDescription.Count);
        Assert.All(productsWithoutDescription, p => Assert.Null(p.Description));
    }

    [Fact]
    public void FilterBy_CustomFilter_ShouldFilterByPrefixes()
    {
        // Act
        List<TestProduct> result = _context.Products
            .FilterBy("NameAlias:La|Bo")
            .ToList();

        // Assert - should match Laptop and Book
        Assert.Equal(2, result.Count);
        Assert.Contains(result, p => p.Name == "Laptop");
        Assert.Contains(result, p => p.Name == "Book");
    }

    [Fact]
    public void SortBy_CustomSort_ShouldOrderByNameLengthAsc()
    {
        // Act
        List<TestProduct> result = _context.Products
            .SortBy("DummySort:asc")
            .ToList();

        // Assert - shortest name first should be "Book"
        Assert.Equal("Book", result.First().Name);
    }

    [Fact]
    public void FilterBy_CombineOrWithOtherFilters_ShouldWork()
    {
        // Act - Electronics OR Clothing, AND price under $100
        List<TestProduct> result = _context.Products
            .FilterBy("Category:Electronics|Clothing,Price:*-100")
            .ToList();

        // Assert - Should get T-Shirt and Jeans (both Clothing under $100)
        Assert.Equal(2, result.Count);
        Assert.All(result, p => Assert.True(p.Category == ProductCategory.Clothing && p.Price <= 100));
    }
}
