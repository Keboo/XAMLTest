namespace XAMLTest.Generator;

public record VisualElement(
    string Namespace,
    VisualElementType Type,
    IReadOnlyList<Property> DependencyProperties)
{
    public string FullTypeName => $"{Namespace}.{Type.FullName}";
}

public record VisualElementType(string Name, string FullName, bool IsFinal);

public record Property(string Name, string TypeFullName, bool CanRead, bool CanWrite);