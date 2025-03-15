using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

namespace Kimmel.IdMismatchEndpointFilter;

public static class IdMismatchEndpointFilterExtensions
{
    public static RouteHandlerBuilder IdMismatch<T>(this RouteHandlerBuilder builder, string routeIdName = "id", string bodyIdName = "Id") =>
        builder.AddEndpointFilter(new PutIdMismatchEndpointFilter<T>(routeIdName, bodyIdName))
            .Produces<string>(StatusCodes.Status400BadRequest);
}
