using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Endpointer.Generator.Tests;

public class EndpointerGeneratorTests
{
    // Stub for ASP.NET Core types needed in test compilation
    private const string AspNetCoreStubs = """
        namespace Microsoft.AspNetCore.Routing
        {
            public interface IEndpointRouteBuilder { }
        }
        namespace Microsoft.Extensions.DependencyInjection
        {
            public interface IServiceCollection { }

            public static class ServiceCollectionExtensions
            {
                public static IServiceCollection AddScoped<T>(this IServiceCollection services) => services;
            }
        }
        """;

    [Test]
    public async Task Generator_EmitsIEndpointInterface()
    {
        var (result, _) = RunGenerator("");

        var iEndpointSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "IEndpoint.g.cs", StringComparison.Ordinal));

        await Assert.That(iEndpointSource.SourceText.ToString()).Contains("public interface IEndpoint");
    }

    [Test]
    public async Task Generator_EmitsMapEndpointerExtension()
    {
        var (result, _) = RunGenerator("");

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        await Assert.That(registrationSource.SourceText.ToString()).Contains("MapEndpointer");
    }

    [Test]
    public async Task Generator_EmitsAddEndpointerExtension()
    {
        var (result, _) = RunGenerator("");

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        await Assert.That(registrationSource.SourceText.ToString()).Contains("AddEndpointer");
    }

    [Test]
    public async Task Generator_RegistersEndpointsInDI()
    {
        const string source = """
            using Endpointer;
            using Microsoft.AspNetCore.Routing;

            namespace TestApp;

            public class GetTimeEndpoint
            {
                public class Endpoint : IEndpoint
                {
                    public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
                }
            }
            """;

        var (result, outputCompilation) = RunGenerator(source);

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        string generatedCode = registrationSource.SourceText.ToString();
        await Assert.That(generatedCode).Contains("services.AddScoped<TestApp.GetTimeEndpoint>();");

        // Verify no compilation errors
        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        await Assert.That(errors).IsEmpty();
    }

    [Test]
    public async Task Generator_DiscoversEndpointImplementation()
    {
        const string source = """
            using Endpointer;
            using Microsoft.AspNetCore.Routing;

            namespace TestApp;

            public class GetTimeEndpoint
            {
                public class Endpoint : IEndpoint
                {
                    public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
                }
            }
            """;

        var (result, _) = RunGenerator(source);

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        string generatedCode = registrationSource.SourceText.ToString();
        await Assert.That(generatedCode).Contains("TestApp.GetTimeEndpoint");
    }

    [Test]
    public async Task Generator_IgnoresNonNestedClasses()
    {
        const string source = """
            using Endpointer;
            using Microsoft.AspNetCore.Routing;

            namespace TestApp;

            public class NotNestedEndpoint : IEndpoint
            {
                public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
            }
            """;

        var (result, _) = RunGenerator(source);

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        string generatedCode = registrationSource.SourceText.ToString();
        await Assert.That(generatedCode).DoesNotContain("NotNestedEndpoint");
    }

    [Test]
    public async Task Generator_RegistersMultipleEndpoints()
    {
        const string source = """
            using Endpointer;
            using Microsoft.AspNetCore.Routing;

            namespace TestApp;

            public class GetTimeEndpoint
            {
                public class Endpoint : IEndpoint
                {
                    public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
                }
            }

            public class CreateUserEndpoint
            {
                public class Endpoint : IEndpoint
                {
                    public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
                }
            }
            """;

        var (result, outputCompilation) = RunGenerator(source);

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        string generatedCode = registrationSource.SourceText.ToString();
        await Assert.That(generatedCode).Contains("services.AddScoped<TestApp.GetTimeEndpoint>();");
        await Assert.That(generatedCode).Contains("services.AddScoped<TestApp.CreateUserEndpoint>();");

        // Verify no compilation errors
        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        await Assert.That(errors).IsEmpty();
    }

    [Test]
    public async Task Generator_HandlesEndpointsInDifferentNamespaces()
    {
        const string source = """
            using Endpointer;
            using Microsoft.AspNetCore.Routing;

            namespace App.Users
            {
                public class GetUserEndpoint
                {
                    public class Endpoint : IEndpoint
                    {
                        public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
                    }
                }
            }

            namespace App.Orders
            {
                public class GetOrderEndpoint
                {
                    public class Endpoint : IEndpoint
                    {
                        public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
                    }
                }
            }
            """;

        var (result, outputCompilation) = RunGenerator(source);

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        string generatedCode = registrationSource.SourceText.ToString();
        await Assert.That(generatedCode).Contains("services.AddScoped<App.Users.GetUserEndpoint>();");
        await Assert.That(generatedCode).Contains("services.AddScoped<App.Orders.GetOrderEndpoint>();");

        // Verify no compilation errors
        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        await Assert.That(errors).IsEmpty();
    }

    [Test]
    public async Task Generator_SupportsNonDefaultNestedClassName()
    {
        const string source = """
            using Endpointer;
            using Microsoft.AspNetCore.Routing;

            namespace TestApp;

            public class GetTimeEndpoint
            {
                public class Handler : IEndpoint
                {
                    public void MapEndpoint(IEndpointRouteBuilder endpoints) { }
                }
            }
            """;

        var (result, outputCompilation) = RunGenerator(source);

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        string generatedCode = registrationSource.SourceText.ToString();
        await Assert.That(generatedCode).Contains("services.AddScoped<TestApp.GetTimeEndpoint>();");
        await Assert.That(generatedCode).Contains("new TestApp.GetTimeEndpoint.Handler()");

        // Verify no compilation errors
        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();
        await Assert.That(errors).IsEmpty();
    }

    [Test]
    public async Task Generator_GeneratesCompilableCode_WithNoEndpoints()
    {
        var (_, outputCompilation) = RunGenerator("");

        var errors = outputCompilation.GetDiagnostics()
            .Where(d => d.Severity == DiagnosticSeverity.Error)
            .ToList();

        await Assert.That(errors).IsEmpty();
    }

    private static (GeneratorDriverRunResult Result, Compilation OutputCompilation) RunGenerator(string source)
    {
        var syntaxTrees = new List<SyntaxTree>
        {
            CSharpSyntaxTree.ParseText(AspNetCoreStubs),
        };

        if (!string.IsNullOrEmpty(source))
        {
            syntaxTrees.Add(CSharpSyntaxTree.ParseText(source));
        }

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: syntaxTrees,
            references: Net80.References.All,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new EndpointerGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out var outputCompilation, out _);

        return (driver.GetRunResult(), outputCompilation);
    }
}
