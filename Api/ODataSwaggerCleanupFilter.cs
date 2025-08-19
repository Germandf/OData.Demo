using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class ODataSwaggerCleanupFilter : IDocumentFilter
{
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext _)
    {
        var remove = swaggerDoc.Paths.Keys
            .Where(p => p.EndsWith("/$count", StringComparison.OrdinalIgnoreCase)
                     || p.Contains("/{key}", StringComparison.Ordinal))  // drop slash-key variant
            .ToList();

        foreach (var path in remove)
            swaggerDoc.Paths.Remove(path);
    }
}