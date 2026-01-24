using Endpointer.TestWebApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Endpointer.TestWebApi.Endpoints;

public class CreateUserEndpoint(ILogger<CreateUserEndpoint> logger, TimeProvider timeProvider)
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/users", (CreateUserEndpoint ep, CreateUserRequest request) => ep.HandleAsync(request));
        }
    }

    public async Task<Created<UserResponse>> HandleAsync(CreateUserRequest request)
    {
        await Task.Delay(1);

        logger.LogInformation("Creating user {Name}", request.Name);

        var response = new UserResponse(
            Id: Random.Shared.Next(1, 1000),
            Name: request.Name,
            Email: request.Email,
            CreatedAt: timeProvider.GetUtcNow());

        return TypedResults.Created($"/users/{response.Id}", response);
    }
}
