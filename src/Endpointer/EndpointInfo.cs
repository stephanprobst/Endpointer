namespace Endpointer;

/// <summary>
/// Immutable class for caching endpoint information in the incremental generator.
/// Value equality is required for proper incremental generation caching.
/// </summary>
internal sealed class EndpointInfo(
    string outerClassName,
    string outerClassNamespace,
    string fullyQualifiedOuterName,
    string nestedEndpointName,
    IReadOnlyList<string> constructorParameterTypes)
{
    public string OuterClassName { get; } = outerClassName;
    public string OuterClassNamespace { get; } = outerClassNamespace;
    public string FullyQualifiedOuterName { get; } = fullyQualifiedOuterName;
    public string NestedEndpointName { get; } = nestedEndpointName;
    public IReadOnlyList<string> ConstructorParameterTypes { get; } = constructorParameterTypes;

    public override bool Equals(object? obj)
    {
        if (obj is not EndpointInfo other)
        {
            return false;
        }

        if (OuterClassName != other.OuterClassName ||
            OuterClassNamespace != other.OuterClassNamespace ||
            FullyQualifiedOuterName != other.FullyQualifiedOuterName ||
            NestedEndpointName != other.NestedEndpointName ||
            ConstructorParameterTypes.Count != other.ConstructorParameterTypes.Count)
        {
            return false;
        }

        for (int i = 0; i < ConstructorParameterTypes.Count; i++)
        {
            if (ConstructorParameterTypes[i] != other.ConstructorParameterTypes[i])
            {
                return false;
            }
        }

        return true;
    }

    public override int GetHashCode()
    {
        int hash = OuterClassName.GetHashCode();
        hash = hash * 31 + OuterClassNamespace.GetHashCode();
        hash = hash * 31 + FullyQualifiedOuterName.GetHashCode();
        hash = hash * 31 + NestedEndpointName.GetHashCode();
        hash = hash * 31 + ConstructorParameterTypes.Count.GetHashCode();
        return hash;
    }
}
