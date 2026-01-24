using Endpointer.TestWebApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Endpointer.TestWebApi.Endpoints;

public class SearchUsersEndpoint(ILogger<SearchUsersEndpoint> logger)
{
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/users", (SearchUsersEndpoint ep, string? search, int? limit) => ep.HandleAsync(search, limit));
        }
    }

    public async Task<Ok<List<UserResponse>>> HandleAsync(string? search, int? limit)
    {
        await Task.Delay(1);

        logger.LogInformation("Searching users with term '{Search}' and limit {Limit}", search, limit);

        int count = limit ?? 10;
        var users = Enumerable.Range(1, count)
            .Select(i => new UserResponse(
                Id: i,
                Name: string.IsNullOrEmpty(search) ? $"User {i}" : $"{search} {i}",
                Email: $"user{i}@example.com",
                CreatedAt: DateTimeOffset.UtcNow))
            .ToList();

        return TypedResults.Ok(users);
    }
}
