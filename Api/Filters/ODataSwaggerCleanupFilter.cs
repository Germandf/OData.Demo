using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class ODataSwaggerCleanupFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swagger, DocumentFilterContext _)
    {
        // Discover OData root + canonical paths from whatever prefix is configured
        var (metadataPaths, serviceDocPaths) = ComputeODataPaths(swagger);

        foreach (var (path, item) in swagger.Paths.ToList())
        {
            bool isMetadata = metadataPaths.Contains(path);
            bool isServiceDoc = serviceDocPaths.Contains(path);

            foreach (var op in item.Operations.Values)
            {
                if (isMetadata)
                {
                    if (!string.IsNullOrWhiteSpace(op.OperationId) && op.OperationId.Contains('$'))
                    {
                        op.OperationId = "GetMetadata";
                    }

                    op.RequestBody = null;
                    foreach (var r in op.Responses.Values)
                    {
                        r.Content.Clear();
                        r.Content["application/xml"] = new OpenApiMediaType
                        {
                            Schema = new OpenApiSchema { Type = "string" } // avoid IEdmModel refs
                        };
                    }
                    continue;
                }

                if (isServiceDoc)
                {
                    if (string.IsNullOrWhiteSpace(op.OperationId) || op.OperationId.Equals("metadata", StringComparison.OrdinalIgnoreCase) || op.OperationId.Equals("Metadata", StringComparison.Ordinal))
                    {
                        op.OperationId = "GetServiceDocument";
                    }

                    op.RequestBody = null;
                    foreach (var r in op.Responses.Values)
                    {
                        r.Content.Clear();
                        r.Content["application/json"] = new OpenApiMediaType
                        {
                            Schema = BuildServiceDocumentSchema() // avoid ODataServiceDocument refs
                        };
                    }
                    continue;
                }

                NormalizeToJson(op.RequestBody?.Content);
                foreach (var r in op.Responses.Values)
                    NormalizeToJson(r.Content);
            }
        }

        // Remove other OData "$..." endpoints but keep $metadata (any prefix)
        var removePaths = swagger.Paths.Keys
            .Where(p => p.Contains("$", StringComparison.Ordinal) && !metadataPaths.Contains(p))
            .ToList();
        foreach (var p in removePaths)
            swagger.Paths.Remove(p);

        // Remove noisy Edm/IEdm/OData helper schemas
        var removeSchemas = swagger.Components.Schemas.Keys
            .Where(k => k.StartsWith("Edm", StringComparison.Ordinal) ||
                        k.StartsWith("IEdm", StringComparison.Ordinal) ||
                        k.StartsWith("OData", StringComparison.Ordinal))
            .ToList();
        foreach (var k in removeSchemas)
            swagger.Components.Schemas.Remove(k);
    }

    private static (HashSet<string> metadataPaths, HashSet<string> serviceDocPaths)
        ComputeODataPaths(OpenApiDocument swagger)
    {
        var ord = StringComparer.OrdinalIgnoreCase;

        // Find any "$metadata" path and derive the prefix from it
        var metaKey = swagger.Paths.Keys
            .FirstOrDefault(k => k.Equals("$metadata", StringComparison.OrdinalIgnoreCase)
                              || k.EndsWith("/$metadata", StringComparison.OrdinalIgnoreCase));

        // Build all canonical variants we want to treat as $metadata and service doc
        var metadata = new HashSet<string>(ord);
        var service = new HashSet<string>(ord);

        if (metaKey is null)
        {
            // Fallback: handle both no-prefix and common "/odata"
            metadata.UnionWith(new[] { "$metadata", "/$metadata", "/odata/$metadata" });
            service.UnionWith(new[] { "/", "/odata", "/odata/" });
            return (metadata, service);
        }

        // Derive prefix (everything before "$metadata"), then normalize slashes
        var before = metaKey[..metaKey.LastIndexOf("$metadata", StringComparison.OrdinalIgnoreCase)];
        var prefix = before.Trim('/'); // "" when no prefix

        if (string.IsNullOrEmpty(prefix))
        {
            metadata.UnionWith(new[] { "$metadata", "/$metadata" });
            service.UnionWith(new[] { "/" });
        }
        else
        {
            metadata.UnionWith(new[]
            {
                $"/{prefix}/$metadata",
                $"{prefix}/$metadata" // some generators omit leading slash
            });
            service.UnionWith(new[]
            {
                $"/{prefix}",
                $"/{prefix}/"
            });
        }

        return (metadata, service);
    }

    private static void NormalizeToJson(IDictionary<string, OpenApiMediaType>? content)
    {
        if (content is null || content.Count == 0) return;

        var json = content
            .Where(kv => kv.Key.StartsWith("application/json", StringComparison.OrdinalIgnoreCase))
            .Select(kv => kv.Value)
            .FirstOrDefault();

        content.Clear();
        if (json is not null)
            content["application/json"] = json;
    }

    private static OpenApiSchema BuildServiceDocumentSchema() =>
        new OpenApiSchema
        {
            Type = "object",
            Properties = new Dictionary<string, OpenApiSchema>
            {
                ["@odata.context"] = new OpenApiSchema { Type = "string" },
                ["value"] = new OpenApiSchema
                {
                    Type = "array",
                    Items = new OpenApiSchema
                    {
                        Type = "object",
                        Properties = new Dictionary<string, OpenApiSchema>
                        {
                            ["name"] = new OpenApiSchema { Type = "string" },
                            ["kind"] = new OpenApiSchema { Type = "string" }, // e.g. "EntitySet"
                            ["url"] = new OpenApiSchema { Type = "string" }
                        },
                        AdditionalPropertiesAllowed = true
                    }
                }
            },
            AdditionalPropertiesAllowed = true
        };
}
