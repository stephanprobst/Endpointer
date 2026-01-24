using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Endpointer.Generator.Tests;

public class EndpointerGeneratorTests
{
    [Test]
    public async Task Generator_EmitsIEndpointInterface()
    {
        const string source = "";

        var result = RunGenerator(source);

        var iEndpointSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "IEndpoint.g.cs", StringComparison.Ordinal));

        await Assert.That(iEndpointSource.SourceText.ToString()).Contains("public interface IEndpoint");
    }

    [Test]
    public async Task Generator_EmitsMapEndpointerExtension()
    {
        const string source = "";

        var result = RunGenerator(source);

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        await Assert.That(registrationSource.SourceText.ToString()).Contains("MapEndpointer");
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
                    public void MapEndpoint(IEndpointRouteBuilder endpoints)
                    {
                        endpoints.MapGet("/time", () => "ok");
                    }
                }
            }
            """;

        var result = RunGenerator(source);

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
                public void MapEndpoint(IEndpointRouteBuilder endpoints)
                {
                    endpoints.MapGet("/test", () => "ok");
                }
            }
            """;

        var result = RunGenerator(source);

        var registrationSource = result.Results[0].GeneratedSources
            .First(s => string.Equals(s.HintName, "EndpointerRegistration.g.cs", StringComparison.Ordinal));

        string generatedCode = registrationSource.SourceText.ToString();
        await Assert.That(generatedCode).DoesNotContain("NotNestedEndpoint");
    }

    private static GeneratorDriverRunResult RunGenerator(string source)
    {
        var syntaxTree = CSharpSyntaxTree.ParseText(source);

        var references = new List<PortableExecutableReference>
        {
            MetadataReference.CreateFromFile(typeof(object).Assembly.Location),
        };

        // Add runtime assemblies
        string runtimeDir = Path.GetDirectoryName(typeof(object).Assembly.Location)!;
        references.Add(MetadataReference.CreateFromFile(Path.Combine(runtimeDir, "System.Runtime.dll")));

        var compilation = CSharpCompilation.Create(
            assemblyName: "TestAssembly",
            syntaxTrees: [syntaxTree],
            references: references,
            options: new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary));

        var generator = new EndpointerGenerator();

        GeneratorDriver driver = CSharpGeneratorDriver.Create(generator);
        driver = driver.RunGeneratorsAndUpdateCompilation(compilation, out _, out _);

        return driver.GetRunResult();
    }
}
