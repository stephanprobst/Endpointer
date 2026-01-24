using Endpointer.TestWebApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Endpointer.TestWebApi.Endpoints;

public class GetUserEndpoint
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/users/{id:int}", (GetUserEndpoint ep, int id) => ep.Handle(id));
        }
    }

    public Results<Ok<UserResponse>, NotFound> Handle(int id)
    {
        if (id <= 0)
        {
            return TypedResults.NotFound();
        }

        var response = new UserResponse(
            Id: id,
            Name: $"User {id}",
            Email: $"user{id}@example.com",
            CreatedAt: DateTimeOffset.UtcNow);

        return TypedResults.Ok(response);
    }
}
