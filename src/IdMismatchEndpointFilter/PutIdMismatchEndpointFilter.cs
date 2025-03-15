using Microsoft.AspNetCore.Http;

namespace Kimmel.IdMismatchEndpointFilter;

public class PutIdMismatchEndpointFilter<T>(string routeIdName = "id", string bodyIdName = "Id") : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        if (context.Arguments.OfType<T>().FirstOrDefault() is not T model)
        {
            return TypedResults.BadRequest("Request body is missing");
        }

        var bodyIdProperty = typeof(T).GetProperty(bodyIdName);
        if (bodyIdProperty == null)
        {
            return TypedResults.BadRequest($"Property '{bodyIdName}' not found on model");
        }
        
        var bodyId = bodyIdProperty.GetValue(model)?.ToString();
        var routeId = context.HttpContext.Request.RouteValues[routeIdName]?.ToString();

        if (!string.Equals(routeId, bodyId, StringComparison.OrdinalIgnoreCase))
        {
            return TypedResults.BadRequest($"Route '{routeIdName}' value does not match model '{bodyIdName}' value");
        }

        return await next(context);
    }
}
