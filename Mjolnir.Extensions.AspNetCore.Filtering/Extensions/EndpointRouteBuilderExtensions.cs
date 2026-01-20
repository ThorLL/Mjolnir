using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using Mjolnir.Extensions.AspNetCore.Filtering.Attributes;

namespace Mjolnir.Extensions.AspNetCore.Filtering.Extensions;

public record OptionResponse(string Name, string Description);

public static class EndpointRouteBuilderExtensions
{
    internal static IEnumerable<OptionResponse> GetFilterableProps<T>() => typeof(T)
        .GetProperties()
        .Select(prop =>
        {
            BaseAttribute? attr = prop.GetCustomAttribute<FilterableAttribute>();
            attr ??= prop.GetCustomAttribute<CustomFilterAttribute<T>>();
            attr?.Validate(prop);
            return (prop, attr);
        })
        .Where(v => v.attr is not null)
        .Select(v => (name: v.prop.Name, description: v.attr!.GetDescription(v.prop)))
        .MustBeUnique(v => v.name)
        .Select(v => new OptionResponse(v.name, v.description));

    internal static IEnumerable<OptionResponse> GetSortableProps<T>() => typeof(T)
        .GetProperties()
        .Select(prop =>
        {
            BaseAttribute? attr = prop.GetCustomAttribute<SortableAttribute>();
            attr ??= prop.GetCustomAttribute<CustomSortingAttribute<T>>();
            attr?.Validate(prop);
            return (prop, attr);
        })
        .Where(v => v.attr is not null)
        .Select(v => (name: v.prop.Name, description: v.attr!.GetDescription(v.prop)))
        .MustBeUnique(v => v.name)
        .Select(v => new OptionResponse(v.name, v.description));

    extension(IEndpointRouteBuilder app)
    {
        public RouteHandlerBuilder MapFilterOptions<T>([StringSyntax("Route")] string pattern, string entityName)
        {
            IEnumerable<OptionResponse> response = GetFilterableProps<T>();

            return app.MapGet(pattern, () => response)
                .Produces<OptionResponse[]>()
                .WithDescription(
                    $"""
                     Retrieves the list of filterable properties for {entityName} and their usage instructions.<br>
                     <br>
                     To filter {entityName}s, use the `filterBy` query parameter:<br>
                     `?filterBy=[property]:[value],[property]:[value],...`<br>
                     <br>
                     Features:<br>
                     - **AND Logic**: Multiple filters separated by commas are combined with AND.<br>
                     - **OR Logic**: Multiple values for a single property can often be combined with `|` (e.g., `job:engineer|doctor`).<br>
                     - **Ranges**: Numeric and date properties support ranges with `-` (e.g., `age:18-25`).<br>
                     - **Unbounded Ranges**: Use `*` for open ranges (e.g., `price:100-*` for ≥ 100).<br>
                     <br>
                     Example:<br>
                     `?filterBy=category:electronics|books,price:10-*,status:active`
                     """);
        }

        public RouteHandlerBuilder MapSortOptions<T>([StringSyntax("Route")] string pattern, string entityName)
        {
            IEnumerable<OptionResponse> response = GetSortableProps<T>();

            return app.MapGet(pattern, () => response)
                .Produces<OptionResponse[]>()
                .WithDescription(
                    $"""
                     Retrieves the list of sortable properties for {entityName} and their usage instructions.<br>
                     <br>
                     To sort {entityName}s, use the `sortBy` query parameter:<br>
                     `?sortBy=[property]:[direction],[property]:[direction],...`<br>
                     <br>
                     Directions are `asc` (ascending) or `desc` (descending).<br>
                     Sorting happens in the order specified.<br>
                     <br>
                     Example:<br>
                     `?sortBy=lastName:asc,firstName:asc,createdAt:desc`
                     """);
        }
    }
}
