namespace XAMLTest.Generator;

public record VisualElement(
    string Namespace,
    VisualElementType Type,
    IReadOnlyList<Property> DependencyProperties)
{ }
