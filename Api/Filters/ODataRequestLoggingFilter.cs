using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.OData.Routing.Controllers;

public sealed class ODataRequestLoggingFilter(ILogger<ODataRequestLoggingFilter> logger) : IActionFilter
{
    public void OnActionExecuting(ActionExecutingContext context)
    {
        if (context.Controller is not ODataController) return;
        var req = context.HttpContext.Request;
        logger.LogInformation("{Method} {Url}", req.Method, Uri.UnescapeDataString(req.GetDisplayUrl()));
    }

    public void OnActionExecuted(ActionExecutedContext _) { }
}
