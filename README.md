# Endpointer

[![Build](https://github.com/stephanprobst/Endpointer/actions/workflows/build.yml/badge.svg)](https://github.com/stephanprobst/Endpointer/actions/workflows/build.yml)
[![NuGet](https://img.shields.io/nuget/v/Endpointer.svg)](https://www.nuget.org/packages/Endpointer/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](https://opensource.org/licenses/MIT)
[![netstandard2.0](https://img.shields.io/badge/netstandard-2.0-blue)](https://docs.microsoft.com/en-us/dotnet/standard/net-standard)
[![Roslyn](https://img.shields.io/badge/Roslyn-Source%20Generator-blueviolet)](https://docs.microsoft.com/en-us/dotnet/csharp/roslyn-sdk/)

A **C# source generator** for ASP.NET Core Minimal APIs implementing the **REPR (Request-Endpoint-Response) pattern**.

Endpointer is a thin layer above ASP.NET Core - not a framework. It generates the boilerplate at compile time with no reflection, while you keep full control over your endpoints.

## Features

- **Zero runtime overhead** - All code is generated at compile time
- **REPR pattern** - Clean separation with Request, Endpoint, and Response in one file
- **Automatic DI registration** - Primary constructor parameters are auto-registered
- **Automatic route mapping** - All endpoints discovered and mapped via source generation
- **Native ASP.NET Core** - Uses `TypedResults`, `IEndpointRouteBuilder`, and standard middleware
- **Incremental generator** - Fast builds with Roslyn's latest `IIncrementalGenerator` API
- **No reflection** - Everything resolved at compile time

## Quick Start

### 1. Install the package

```bash
dotnet add package Endpointer
```

### 2. Create an endpoint

```csharp
using Microsoft.AspNetCore.Http.HttpResults;

public class GetTimeEndpoint(TimeProvider timeProvider)
{
    public record GetTimeResponse(DateTimeOffset CurrentTime);

    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapGet("/time", (GetTimeEndpoint ep) => ep.Handle());
        }
    }

    public Ok<GetTimeResponse> Handle()
    {
        return TypedResults.Ok(new GetTimeResponse(timeProvider.GetUtcNow()));
    }
}
```

### 3. Register in Program.cs

```csharp
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointer();  // Generated - registers all endpoint classes

var app = builder.Build();

app.MapEndpointer();  // Generated - maps all endpoints

await app.RunAsync();
```

That's it! The source generator discovers all `IEndpoint` implementations and generates the registration code.

## How It Works

Endpointer uses Roslyn's incremental source generator to:

1. **Discover endpoints** - Finds all nested classes implementing `IEndpoint`
2. **Extract metadata** - Captures the outer class name and its primary constructor parameters
3. **Generate registration** - Creates extension methods for DI and route mapping

### Generated Code

The generator produces two files:

**IEndpoint.g.cs** - The marker interface:
```csharp
public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder endpoints);
}
```

**EndpointerRegistration.g.cs** - Extension methods:
```csharp
public static class EndpointerExtensions
{
    public static IServiceCollection AddEndpointer(this IServiceCollection services)
    {
        services.AddScoped<GetTimeEndpoint>();
        services.AddScoped<GetUserEndpoint>();
        services.AddScoped<CreateUserEndpoint>();
        // ... all discovered endpoints
        return services;
    }

    public static IEndpointRouteBuilder MapEndpointer(this IEndpointRouteBuilder endpoints)
    {
        new GetTimeEndpoint.Endpoint().MapEndpoint(endpoints);
        new GetUserEndpoint.Endpoint().MapEndpoint(endpoints);
        new CreateUserEndpoint.Endpoint().MapEndpoint(endpoints);
        // ... all discovered endpoints
        return endpoints;
    }
}
```

## The REPR Pattern

REPR (Request-Endpoint-Response) organizes API code by feature rather than by layer:

```
Endpoints/
├── Users/
│   ├── CreateUserEndpoint.cs    # POST /users
│   ├── GetUserEndpoint.cs       # GET /users/{id}
│   ├── UpdateUserEndpoint.cs    # PUT /users/{id}
│   └── DeleteUserEndpoint.cs    # DELETE /users/{id}
└── Health/
    └── HealthEndpoint.cs        # GET /health
```

Each file contains:
- **Request** - Input DTOs (records)
- **Endpoint** - Route mapping (nested `IEndpoint` class)
- **Response** - Output DTOs (records)
- **Handler** - Business logic (methods on outer class)

## Requirements

- **.NET 10.0** or later (for the application)
- The generator itself targets **netstandard2.0** for broad compatibility

## Building

```bash
# Build
dotnet build src/Endpointer.slnx

# Test
dotnet test --solution src/Endpointer.slnx
```

## Full Example

A complete endpoint with request/response DTOs, dependency injection, and OpenAPI metadata:

```csharp
using Microsoft.AspNetCore.Http.HttpResults;

namespace MyApi.Endpoints.Products;

public class CreateProductEndpoint(IProductRepository repository, ILogger<CreateProductEndpoint> logger)
{
    // Request
    public record CreateProductRequest(
        string Name,
        string Description,
        decimal Price,
        string Category);

    // Response
    public record ProductResponse(
        int Id,
        string Name,
        string Description,
        decimal Price,
        string Category,
        DateTimeOffset CreatedAt);

    // Endpoint
    public class Endpoint : IEndpoint
    {
        public void MapEndpoint(IEndpointRouteBuilder endpoints)
        {
            endpoints.MapPost("/products", (CreateProductEndpoint ep, CreateProductRequest request) => ep.HandleAsync(request))
                .WithName("CreateProduct")
                .WithTags("Products")
                .WithSummary("Create a new product")
                .WithDescription("Creates a new product in the catalog and returns the created product with its assigned ID.")
                .Produces<ProductResponse>(StatusCodes.Status201Created)
                .ProducesValidationProblem()
                .WithOpenApi();
        }
    }

    // Handler
    public async Task<Results<Created<ProductResponse>, ValidationProblem>> HandleAsync(CreateProductRequest request)
    {
        if (request.Price < 0)
        {
            return TypedResults.ValidationProblem(new Dictionary<string, string[]>
            {
                ["Price"] = ["Price must be greater than or equal to zero."]
            });
        }

        logger.LogInformation("Creating product {Name} in category {Category}", request.Name, request.Category);

        var product = await repository.CreateAsync(request);

        var response = new ProductResponse(
            product.Id,
            product.Name,
            product.Description,
            product.Price,
            product.Category,
            product.CreatedAt);

        return TypedResults.Created($"/products/{response.Id}", response);
    }
}
```

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
