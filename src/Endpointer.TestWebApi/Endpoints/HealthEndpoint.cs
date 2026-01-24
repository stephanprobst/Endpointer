using Microsoft.AspNetCore.Http.HttpResults;

namespace Endpointer.TestWebApi.Endpoints;

#pragma warning disable S2325 // Methods should not be instance-specific when they don't access instance data - REPR pattern requires instance methods
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
