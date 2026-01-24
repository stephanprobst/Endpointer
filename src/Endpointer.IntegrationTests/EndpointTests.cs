using System.Net;
using System.Net.Http.Json;
using Endpointer.TestWebApi.Models;

namespace Endpointer.IntegrationTests;

public class EndpointTests
{
    [Test]
    public async Task HealthEndpoint_ReturnsHealthy()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/health");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        string content = await response.Content.ReadAsStringAsync();
        await Assert.That(content).IsEqualTo("\"Healthy\"");
    }

    [Test]
    public async Task GetTimeEndpoint_ReturnsCurrentTime()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/time");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var json = await response.Content.ReadFromJsonAsync<GetTimeResponse>();
        await Assert.That(json).IsNotNull();
        await Assert.That(json!.CurrentTime).IsEqualTo(factory.TimeProvider.GetUtcNow());
    }

    [Test]
    public async Task CreateUserEndpoint_ReturnsCreatedWithLocation()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();
        var request = new CreateUserRequest("John Doe", "john@example.com");

        var response = await client.PostAsJsonAsync("/users", request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.Created);
        await Assert.That(response.Headers.Location).IsNotNull();
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Name).IsEqualTo("John Doe");
        await Assert.That(user.Email).IsEqualTo("john@example.com");
    }

    [Test]
    public async Task GetUserEndpoint_WithValidId_ReturnsUser()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/users/42");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Id).IsEqualTo(42);
    }

    [Test]
    public async Task GetUserEndpoint_WithInvalidId_ReturnsNotFound()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/users/0");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task UpdateUserEndpoint_WithValidId_ReturnsUpdatedUser()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();
        var request = new UpdateUserRequest("Jane Doe", "jane@example.com");

        var response = await client.PutAsJsonAsync("/users/42", request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var user = await response.Content.ReadFromJsonAsync<UserResponse>();
        await Assert.That(user).IsNotNull();
        await Assert.That(user!.Id).IsEqualTo(42);
        await Assert.That(user.Name).IsEqualTo("Jane Doe");
    }

    [Test]
    public async Task UpdateUserEndpoint_WithInvalidId_ReturnsNotFound()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();
        var request = new UpdateUserRequest("Jane Doe", "jane@example.com");

        var response = await client.PutAsJsonAsync("/users/0", request);

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task DeleteUserEndpoint_WithValidId_ReturnsNoContent()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync("/users/42");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NoContent);
    }

    [Test]
    public async Task DeleteUserEndpoint_WithInvalidId_ReturnsNotFound()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.DeleteAsync("/users/0");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.NotFound);
    }

    [Test]
    public async Task SearchUsersEndpoint_ReturnsUserList()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/users");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        await Assert.That(users).IsNotNull();
        await Assert.That(users!.Count).IsEqualTo(10); // Default limit
    }

    [Test]
    public async Task SearchUsersEndpoint_WithLimit_RespectsLimit()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/users?limit=5");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        await Assert.That(users).IsNotNull();
        await Assert.That(users!.Count).IsEqualTo(5);
    }

    [Test]
    public async Task SearchUsersEndpoint_WithSearch_FiltersResults()
    {
        await using var factory = new TestWebApiFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/users?search=Test&limit=3");

        await Assert.That(response.StatusCode).IsEqualTo(HttpStatusCode.OK);
        var users = await response.Content.ReadFromJsonAsync<List<UserResponse>>();
        await Assert.That(users).IsNotNull();
        await Assert.That(users!.Count).IsEqualTo(3);
        await Assert.That(users[0].Name).Contains("Test");
    }

    private sealed record GetTimeResponse(DateTimeOffset CurrentTime);
}
