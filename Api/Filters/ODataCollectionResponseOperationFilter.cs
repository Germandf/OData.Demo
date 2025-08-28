using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class ODataCollectionResponseOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (!"GET".Equals(context.ApiDescription.HttpMethod, StringComparison.OrdinalIgnoreCase)) return;

        var type = context.MethodInfo.DeclaringType;
        var isODataController = type != null && typeof(ODataController).IsAssignableFrom(type);
        if (!isODataController) return;

        foreach (var response in operation.Responses.Values)
        {
            if (response.Content == null) continue;
            foreach (var media in response.Content.Values)
            {
                var schema = media.Schema;
                if (schema == null)
                    continue;
                if (schema.Type == "array")
                    media.Schema = WrapCollection(schema);
            }
        }
    }

    private static OpenApiSchema WrapCollection(OpenApiSchema arraySchema) => new OpenApiSchema
    {
        Type = "object",
        Properties = new Dictionary<string, OpenApiSchema>
        {
            ["@odata.context"] = new OpenApiSchema { Type = "string" },
            ["@odata.count"] = new OpenApiSchema { Type = "integer", Format = "int32" },
            ["@odata.nextLink"] = new OpenApiSchema { Type = "string" },
            ["value"] = arraySchema
        },
        AdditionalPropertiesAllowed = true,
        Required = new HashSet<string> { "value" }
    };
}
