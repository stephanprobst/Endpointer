using Endpointer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddEndpointer();

var app = builder.Build();

app.MapEndpointer();

await app.RunAsync();

// Make Program class accessible for WebApplicationFactory in integration tests < .NET 10
#pragma warning disable S1118
public partial class Program;
#pragma warning restore S1118
