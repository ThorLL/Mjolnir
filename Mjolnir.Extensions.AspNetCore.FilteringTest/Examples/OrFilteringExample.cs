using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;
using Mjolnir.Extensions.AspNetCore.FilteringTest.Models;

namespace Mjolnir.Extensions.AspNetCore.FilteringTest.Examples;

/// <summary>
///     Example demonstrating OR filtering functionality using the pipe symbol '|'
/// </summary>
public static class OrFilteringExample
{
    public static void RunExample()
    {
        List<TestProduct> products = new()
        {
            new TestProduct { Name = "Laptop", Price = 999.99m, Category = ProductCategory.Electronics },
            new TestProduct { Name = "T-Shirt", Price = 19.99m, Category = ProductCategory.Clothing },
            new TestProduct { Name = "Book", Price = 12.50m, Category = ProductCategory.Books },
            new TestProduct { Name = "Smartphone", Price = 599.99m, Category = ProductCategory.Electronics },
            new TestProduct { Name = "Jeans", Price = 79.99m, Category = ProductCategory.Clothing }
        };

        Console.WriteLine("=== OR Filtering Examples ===\n");

        // Example 1: String OR filtering
        Console.WriteLine("1. String OR: Names containing 'Laptop' OR 'Book'");
        Console.WriteLine("   Filter: Name:Laptop|Book");
        List<TestProduct> stringOrResults = products.FilterBy("Name:Laptop|Book").ToList();
        foreach (TestProduct product in stringOrResults) Console.WriteLine($"   - {product.Name} (${product.Price})");
        Console.WriteLine();

        // Example 2: Enum OR filtering  
        Console.WriteLine("2. Enum OR: Electronics OR Clothing categories");
        Console.WriteLine("   Filter: Category:Electronics|Clothing");
        List<TestProduct> enumOrResults = products.FilterBy("Category:Electronics|Clothing").ToList();
        foreach (TestProduct product in enumOrResults)
            Console.WriteLine($"   - {product.Name} ({product.Category}) - ${product.Price}");
        Console.WriteLine();

        // Example 3: Combined OR and AND filtering
        Console.WriteLine("3. Combined: (Electronics OR Clothing) AND under $100");
        Console.WriteLine("   Filter: Category:Electronics|Clothing,Price:*-100");
        List<TestProduct> combinedResults = products.FilterBy("Category:Electronics|Clothing,Price:*-100").ToList();
        foreach (TestProduct product in combinedResults)
            Console.WriteLine($"   - {product.Name} ({product.Category}) - ${product.Price}");
        Console.WriteLine();

        // Example 4: OR with spaces (automatically trimmed)
        Console.WriteLine("4. OR with spaces (automatically trimmed)");
        Console.WriteLine("   Filter: 'Category: Electronics | Books '");
        List<TestProduct> spacedResults = products.FilterBy("Category: Electronics | Books ").ToList();
        foreach (TestProduct product in spacedResults)
            Console.WriteLine($"   - {product.Name} ({product.Category}) - ${product.Price}");
    }
}
