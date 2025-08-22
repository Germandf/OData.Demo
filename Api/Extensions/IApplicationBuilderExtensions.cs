using Microsoft.AspNetCore.Http.Extensions;

public static class IApplicationBuilderExtensions
{
    private const string ApiKeyName = "ApiKey";

    public static IApplicationBuilder UseApiKey(this IApplicationBuilder app, string validApiKey)
    {
        return app.Use(async (ctx, next) =>
        {
            if (ctx.Request.Path.StartsWithSegments("/swagger") ||
                ctx.Request.Path == "/" ||
                ctx.Request.Path == "/$metadata")
            {
                await next();
                return;
            }
            var apiKey = ctx.Request.Headers[ApiKeyName].FirstOrDefault() ?? ctx.Request.Query[ApiKeyName].FirstOrDefault();
            if (apiKey != validApiKey)
            {
                ctx.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
            if (ctx.Request.Query.ContainsKey(ApiKeyName))
            {
                var qb = new QueryBuilder();
                foreach (var kv in ctx.Request.Query)
                {
                    if (!string.Equals(kv.Key, ApiKeyName, StringComparison.OrdinalIgnoreCase))
                        foreach (var v in kv.Value) qb.Add(kv.Key, v!);
                }
                ctx.Request.QueryString = qb.ToQueryString();
            }
            await next();
        });
    }
}
