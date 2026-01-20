using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.DependencyInjection;
using Mjolnir.Extensions.AspNetCore.Filtering.Extensions;
using Mjolnir.Extensions.AspNetCore.FilteringTest.Models;
using FilteringExtensions = Mjolnir.Extensions.AspNetCore.Filtering.Extensions.EndpointRouteBuilderExtensions;

namespace Mjolnir.Extensions.AspNetCore.FilteringTest;

public class EndpointRouteBuilderExtensionsTests
{
    [Fact]
    public async Task MapFilterOptions_ShouldRegisterRoute()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act
        app.MapFilterOptions<TestProduct>("/test-filters", "TestProduct");
        await app.StartAsync();

        // Assert
        EndpointDataSource dataSource = app.Services.GetRequiredService<EndpointDataSource>();
        Endpoint? endpoint =
            dataSource.Endpoints.FirstOrDefault(e =>
                e is RouteEndpoint re && re.RoutePattern.RawText == "/test-filters");

        Assert.NotNull(endpoint);
        Assert.Equal("GET",
            (endpoint as RouteEndpoint)?.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.FirstOrDefault());

        // Verify response
        using HttpClient client = new();
        string address = app.Urls.First();
        HttpResponseMessage response = await client.GetAsync($"{address}/test-filters");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<OptionResponse>? content = await response.Content.ReadFromJsonAsync<List<OptionResponse>>();
        Assert.NotNull(content);
        Assert.Equal(7, content.Count);
        Assert.Contains(content, p => p is { Name: "Name", Description: "Filter by `Name` using substring match: `filterBy=Name:value`. Multiple values can be combined with `|` (OR logic)." });
        Assert.Contains(content, p => p is { Name: "Price", Description: "Filter by `Price` using ranges: `filterBy=Price:min-max` (inclusive) or `filterBy=Price:min-*` / `filterBy=Price:*-max`." });
        Assert.Contains(content, p => p is { Name: "Stock", Description: "Filter by `Stock` using ranges: `filterBy=Stock:min-max` (inclusive) or `filterBy=Stock:min-*` / `filterBy=Stock:*-max`." });
        Assert.Contains(content, p => p is { Name: "Category", Description: "Filter by `Category` using enum `ProductCategory` values: `filterBy=Category:Electronics|Clothing|...Sports`. Multiple values can be combined with `|` (OR logic)." });
        Assert.Contains(content, p => p is { Name: "IsActive", Description: "Filter by `IsActive` using boolean values: `filterBy=IsActive:true|false`." });
        Assert.Contains(content, p => p is { Name: "Description", Description: "Filter by `Description` for nullability: `filterBy=Description:true|false` (true: NOT NULL, false: NULL)." });
        Assert.Contains(content, p => p is { Name: "NameAlias", Description: "Custom test filter: starts with (supports v1|v2 OR)" });
        Assert.DoesNotContain(content, p => p is { Name: "Id"});
        Assert.DoesNotContain(content, p => p is { Name: "CreatedAt"});

        await app.StopAsync();
    }

    [Fact]
    public async Task MapSortOptions_ShouldRegisterRoute()
    {
        // Arrange
        WebApplicationBuilder builder = WebApplication.CreateBuilder();
        WebApplication app = builder.Build();

        // Act
        app.MapSortOptions<TestProduct>("/test-sorts", "TestProduct");
        await app.StartAsync();

        // Assert
        EndpointDataSource dataSource = app.Services.GetRequiredService<EndpointDataSource>();
        Endpoint? endpoint =
            dataSource.Endpoints.FirstOrDefault(e => e is RouteEndpoint re && re.RoutePattern.RawText == "/test-sorts");

        Assert.NotNull(endpoint);
        Assert.Equal("GET",
            (endpoint as RouteEndpoint)?.Metadata.GetMetadata<IHttpMethodMetadata>()?.HttpMethods.FirstOrDefault());

        // Verify response
        using HttpClient client = new();
        string address = app.Urls.First();
        HttpResponseMessage response = await client.GetAsync($"{address}/test-sorts");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        List<OptionResponse>? content = await response.Content.ReadFromJsonAsync<List<OptionResponse>>();
        Assert.NotNull(content);
        Assert.Equal(6, content.Count);
        Assert.Contains(content, p => p is { Name: "Name", Description: "Sort by `Name` in ascending or descending order: `sortBy=Name:asc|desc`." });
        Assert.Contains(content, p => p is { Name: "Price", Description: "Sort by `Price` in ascending or descending order: `sortBy=Price:asc|desc`." });
        Assert.Contains(content, p => p is { Name: "Stock", Description: "Sort by `Stock` in ascending or descending order: `sortBy=Stock:asc|desc`." });
        Assert.Contains(content, p => p is { Name: "Category", Description: "Sort by `Category` in ascending or descending order: `sortBy=Category:asc|desc`." });
        Assert.Contains(content, p => p is { Name: "CreatedAt", Description: "Sort by `CreatedAt` in ascending or descending order: `sortBy=CreatedAt:asc|desc`." });
        Assert.Contains(content, p => p is { Name: "DummySort", Description: "Custom test sort: by Name length (asc|desc)" });
        Assert.DoesNotContain(content, p => p is { Name: "Id"});
        Assert.DoesNotContain(content, p => p is { Name: "IsActive"});
        Assert.DoesNotContain(content, p => p is { Name: "Description"});

        await app.StopAsync();
    }

    [Fact]
    public void GetFilterableProps_ShouldReturnCorrectProperties()
    {
        // Act
        List<OptionResponse> props = FilteringExtensions.GetFilterableProps<TestProduct>().ToList();

        // Assert
        Assert.NotNull(props);
        Assert.Equal(7, props.Count);
        Assert.Contains(props, p => p is { Name: "Name", Description: "Filter by `Name` using substring match: `filterBy=Name:value`. Multiple values can be combined with `|` (OR logic)." });
        Assert.Contains(props, p => p is { Name: "Price", Description: "Filter by `Price` using ranges: `filterBy=Price:min-max` (inclusive) or `filterBy=Price:min-*` / `filterBy=Price:*-max`." });
        Assert.Contains(props, p => p is { Name: "Stock", Description: "Filter by `Stock` using ranges: `filterBy=Stock:min-max` (inclusive) or `filterBy=Stock:min-*` / `filterBy=Stock:*-max`." });
        Assert.Contains(props, p => p is { Name: "Category", Description: "Filter by `Category` using enum `ProductCategory` values: `filterBy=Category:Electronics|Clothing|...Sports`. Multiple values can be combined with `|` (OR logic)." });
        Assert.Contains(props, p => p is { Name: "IsActive", Description: "Filter by `IsActive` using boolean values: `filterBy=IsActive:true|false`." });
        Assert.Contains(props, p => p is { Name: "Description", Description: "Filter by `Description` for nullability: `filterBy=Description:true|false` (true: NOT NULL, false: NULL)." });
        Assert.Contains(props, p => p is { Name: "NameAlias", Description: "Custom test filter: starts with (supports v1|v2 OR)" });
        Assert.DoesNotContain(props, p => p is { Name: "Id"});
        Assert.DoesNotContain(props, p => p is { Name: "CreatedAt"});
    }

    [Fact]
    public void GetSortableProps_ShouldReturnCorrectProperties()
    {
        // Act
        List<OptionResponse> props = FilteringExtensions.GetSortableProps<TestProduct>().ToList();

        // Assert
        Assert.NotNull(props);
        Assert.Equal(6, props.Count);
        Assert.Contains(props, p => p is { Name: "Name", Description: "Sort by `Name` in ascending or descending order: `sortBy=Name:asc|desc`." });
        Assert.Contains(props, p => p is { Name: "Price", Description: "Sort by `Price` in ascending or descending order: `sortBy=Price:asc|desc`." });
        Assert.Contains(props, p => p is { Name: "Stock", Description: "Sort by `Stock` in ascending or descending order: `sortBy=Stock:asc|desc`." });
        Assert.Contains(props, p => p is { Name: "Category", Description: "Sort by `Category` in ascending or descending order: `sortBy=Category:asc|desc`." });
        Assert.Contains(props, p => p is { Name: "CreatedAt", Description: "Sort by `CreatedAt` in ascending or descending order: `sortBy=CreatedAt:asc|desc`." });
        Assert.Contains(props, p => p is { Name: "DummySort", Description: "Custom test sort: by Name length (asc|desc)" });
        Assert.DoesNotContain(props, p => p is { Name: "Id"});
        Assert.DoesNotContain(props, p => p is { Name: "IsActive"});
        Assert.DoesNotContain(props, p => p is { Name: "Description"});
    }
}
