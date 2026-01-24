namespace Endpointer.TestWebApi.Models;

public record CreateUserRequest(string Name, string Email);

public record UpdateUserRequest(string Name, string Email);

public record UserResponse(int Id, string Name, string Email, DateTimeOffset CreatedAt);
