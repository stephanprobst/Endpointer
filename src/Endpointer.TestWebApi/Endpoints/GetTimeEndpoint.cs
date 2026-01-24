using Microsoft.AspNetCore.Http.HttpResults;

namespace Endpointer.TestWebApi.Endpoints;

public class GetTimeEndpoint(TimeProvider timeProvider)
{
    public record GetTimeResponse(DateTimeOffset CurrentTime);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/time", (GetTimeEndpoint ep) => ep.Handle());
        }
    }

    public Ok<GetTimeResponse> Handle()
    {
        return TypedResults.Ok(new GetTimeResponse(timeProvider.GetUtcNow()));
    }
}
