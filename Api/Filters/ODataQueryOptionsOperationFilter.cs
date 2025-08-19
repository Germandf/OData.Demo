using Microsoft.AspNetCore.OData.Query;
using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class ODataQueryOptionsOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!"GET".Equals(context.ApiDescription.HttpMethod, StringComparison.OrdinalIgnoreCase)) return;

        var type = context.MethodInfo.DeclaringType;
        var isODataController = type != null && typeof(ODataController).IsAssignableFrom(type);
        var hasEnableQuery =
            context.MethodInfo.GetCustomAttributes(true).OfType<EnableQueryAttribute>().Any() ||
            (type?.GetCustomAttributes(true).OfType<EnableQueryAttribute>().Any() ?? false);

        if (!isODataController && !hasEnableQuery) return;

        operation.Parameters ??= new List<OpenApiParameter>();

        void Add(OpenApiParameter p)
        {
            if (!operation.Parameters.Any(x => x.Name.Equals(p.Name, StringComparison.OrdinalIgnoreCase)))
                operation.Parameters.Add(p);
        }

        Add(new OpenApiParameter
        {
            Name = "$select",
            In = ParameterLocation.Query,
            Schema = new OpenApiSchema { Type = "string" }
        });

        Add(new OpenApiParameter
        {
            Name = "$expand",
            In = ParameterLocation.Query,
            Schema = new OpenApiSchema { Type = "string" }
        });

        Add(new OpenApiParameter
        {
            Name = "$filter",
            In = ParameterLocation.Query,
            Schema = new OpenApiSchema { Type = "string" }
        });

        Add(new OpenApiParameter
        {
            Name = "$orderby",
            In = ParameterLocation.Query,
            Schema = new OpenApiSchema { Type = "string" }
        });

        Add(new OpenApiParameter
        {
            Name = "$top",
            In = ParameterLocation.Query,
            Schema = new OpenApiSchema { Type = "integer", Format = "int32", Minimum = 0 }
        });

        Add(new OpenApiParameter
        {
            Name = "$skip",
            In = ParameterLocation.Query,
            Schema = new OpenApiSchema { Type = "integer", Format = "int32", Minimum = 0 }
        });

        Add(new OpenApiParameter
        {
            Name = "$count",
            In = ParameterLocation.Query,
            Schema = new OpenApiSchema { Type = "boolean" }
        });
    }
}
