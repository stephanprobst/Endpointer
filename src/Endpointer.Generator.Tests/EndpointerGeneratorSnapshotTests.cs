using Basic.Reference.Assemblies;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Endpointer.Generator.Tests;

public class EndpointerGeneratorSnapshotTests
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
    public Task IEndpointInterface_MatchesSnapshot()
    {
        var (result, _) = RunGenerator("");

        var generatedSources = result.Results[0].GeneratedSources
            .Where(s => string.Equals(s.HintName, "IEndpoint.g.cs", StringComparison.Ordinal))
            .Select(s => s.SourceText.ToString());

        return Verify(generatedSources);
    }

    [Test]
    public Task EndpointerRegistration_WithNoEndpoints_MatchesSnapshot()
    {
        var (result, _) = RunGenerator("");

        var generatedSources = result.Results[0].GeneratedSources
            .Where(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal))
            .Select(s => s.SourceText.ToString());

        return Verify(generatedSources);
    }

    [Test]
    public Task EndpointerRegistration_WithSingleEndpoint_MatchesSnapshot()
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

        var generatedSources = result.Results[0].GeneratedSources
            .Where(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal))
            .Select(s => s.SourceText.ToString());

        return Verify(generatedSources);
    }

    [Test]
    public Task EndpointerRegistration_WithMultipleEndpoints_MatchesSnapshot()
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

                public class CreateUserEndpoint
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

        var (result, _) = RunGenerator(source);

        var generatedSources = result.Results[0].GeneratedSources
            .Where(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal))
            .Select(s => s.SourceText.ToString());

        return Verify(generatedSources);
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
