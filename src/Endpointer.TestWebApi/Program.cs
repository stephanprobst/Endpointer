using Endpointer;
using Endpointer.TestWebApi.Endpoints;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);

builder.Services.AddScoped<GetTimeEndpoint>();
builder.Services.AddScoped<CreateUserEndpoint>();
builder.Services.AddScoped<GetUserEndpoint>();
builder.Services.AddScoped<UpdateUserEndpoint>();
builder.Services.AddScoped<DeleteUserEndpoint>();
builder.Services.AddScoped<SearchUsersEndpoint>();
builder.Services.AddScoped<HealthEndpoint>();

var app = builder.Build();

app.MapEndpointer();

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}
