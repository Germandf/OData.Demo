using Microsoft.AspNetCore.OData.Routing.Controllers;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

public sealed class ODataOperationIdFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var type = context.MethodInfo.DeclaringType;

        if (type is null || !typeof(ODataController).IsAssignableFrom(type))
            return;

        var opId = operation.OperationId;

        if (string.IsNullOrWhiteSpace(opId))
            operation.OperationId = "Get" + context.MethodInfo.Name;

        else if (!opId.StartsWith("Get", StringComparison.Ordinal))
            operation.OperationId = "Get" + opId;
    }
}
