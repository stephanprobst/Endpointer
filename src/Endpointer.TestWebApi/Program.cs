using Endpointer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSingleton(TimeProvider.System);
builder.Services.AddEndpointer();

var app = builder.Build();

app.MapEndpointer();

await app.RunAsync();
