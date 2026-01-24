using Microsoft.AspNetCore.Http.HttpResults;

namespace Endpointer.TestWebApi.Endpoints;

#pragma warning disable S2325 // REPR pattern requires instance methods
public class DeleteUserEndpoint
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapDelete("/users/{id:int}", (DeleteUserEndpoint ep, int id) => ep.Handle(id));
        }
    }

    public Results<NoContent, NotFound> Handle(int id)
    {
        if (id <= 0)
        {
            return TypedResults.NotFound();
        }

        return TypedResults.NoContent();
    }
}
