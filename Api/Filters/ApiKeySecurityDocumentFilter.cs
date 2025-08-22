using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class ApiKeySecurityDocumentFilter : IDocumentFilter
{
    private const string ApiKeyName = "ApiKey";

    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        swaggerDoc.Components ??= new OpenApiComponents();
        swaggerDoc.Components.SecuritySchemes ??= new Dictionary<string, OpenApiSecurityScheme>();
        swaggerDoc.Components.SecuritySchemes[ApiKeyName] = new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.ApiKey,
            In = ParameterLocation.Header,
            Name = ApiKeyName
        };
        swaggerDoc.SecurityRequirements ??= new List<OpenApiSecurityRequirement>();
        swaggerDoc.SecurityRequirements.Add(new OpenApiSecurityRequirement
        {
            [new OpenApiSecurityScheme
            { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = ApiKeyName } }
            ] = Array.Empty<string>()
        });
    }
}
