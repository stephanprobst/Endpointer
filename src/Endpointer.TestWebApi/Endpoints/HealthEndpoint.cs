using Microsoft.AspNetCore.Http.HttpResults;

namespace Endpointer.TestWebApi.Endpoints;

public class HealthEndpoint
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/health", (HealthEndpoint ep) => ep.Handle());
        }
    }

    public Ok<string> Handle() => TypedResults.Ok("Healthy");
}
