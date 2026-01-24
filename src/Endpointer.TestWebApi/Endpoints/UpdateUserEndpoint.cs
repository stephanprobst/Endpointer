using Endpointer.TestWebApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Endpointer.TestWebApi.Endpoints;

public class UpdateUserEndpoint(TimeProvider timeProvider)
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPut("/users/{id:int}", (UpdateUserEndpoint ep, int id, UpdateUserRequest request) => ep.HandleAsync(id, request));
        }
    }

    public async Task<Results<Ok<UserResponse>, NotFound>> HandleAsync(int id, UpdateUserRequest request)
    {
        await Task.Delay(1);

        if (id <= 0)
        {
            return TypedResults.NotFound();
        }

        var response = new UserResponse(
            Id: id,
            Name: request.Name,
            Email: request.Email,
            CreatedAt: timeProvider.GetUtcNow());

        return TypedResults.Ok(response);
    }
}
