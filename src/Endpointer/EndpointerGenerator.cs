using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Endpointer;

[Generator]
public sealed class EndpointerGenerator : IIncrementalGenerator
{
    private const string IEndpointFullName = "Endpointer.IEndpoint";

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // Always emit the IEndpoint interface
        context.RegisterPostInitializationOutput(static ctx =>
        {
            ctx.AddSource("IEndpoint.g.cs", SourceGenerationHelper.GenerateIEndpointInterface());
        });

        // Stage 1: Filter to nested classes that might implement IEndpoint
        var endpoints = context.SyntaxProvider
            .CreateSyntaxProvider(
                predicate: static (node, _) => IsNestedClassCandidate(node),
                transform: static (ctx, ct) => GetEndpointInfo(ctx, ct))
            .Where(static info => info is not null);

        // Stage 2: Collect all endpoints
        var collected = endpoints.Collect()!;

        // Stage 3: Generate registration code
        context.RegisterSourceOutput(collected, static (ctx, endpoints) =>
        {
            string source = SourceGenerationHelper.GenerateExtensionClass(endpoints!);
            ctx.AddSource("EndpointerRegistration.g.cs", source);
        });
    }

    private static bool IsNestedClassCandidate(SyntaxNode node)
    {
        // Fast syntax-only check: must be a class nested inside another class
        return node is ClassDeclarationSyntax cds
            && cds.Parent is ClassDeclarationSyntax;
    }

    private static EndpointInfo? GetEndpointInfo(
        GeneratorSyntaxContext context,
        CancellationToken ct)
    {
        var classDecl = (ClassDeclarationSyntax)context.Node;
        var symbol = (INamedTypeSymbol?)context.SemanticModel.GetDeclaredSymbol(classDecl, ct);

        if (symbol is null)
        {
            return null;
        }

        // Check if nested class implements IEndpoint
        bool implementsIEndpoint = symbol.AllInterfaces
            .Any(i => i.ToDisplayString() == IEndpointFullName);

        if (!implementsIEndpoint)
        {
            return null;
        }

        // Get outer class info
        var outerClass = symbol.ContainingType;
        if (outerClass is null)
        {
            return null;
        }

        // Extract constructor parameters from outer class for DI
        var primaryCtor = outerClass.InstanceConstructors
            .FirstOrDefault(c => c.Parameters.Length > 0 && !c.IsImplicitlyDeclared);

        var paramTypes = primaryCtor?.Parameters
            .Select(p => p.Type.ToDisplayString())
            .ToList() ?? [];

        return new EndpointInfo(
            outerClassName: outerClass.Name,
            outerClassNamespace: outerClass.ContainingNamespace.ToDisplayString(),
            fullyQualifiedOuterName: outerClass.ToDisplayString(),
            nestedEndpointName: symbol.Name,
            constructorParameterTypes: paramTypes);
    }
}
