namespace XamlTest;

/// <summary>
/// Represents a node in the WPF visual tree.
/// </summary>
public class VisualTreeNodeInfo
{
    /// <summary>
    /// The type name of the element.
    /// </summary>
    public required string Type { get; init; }

    /// <summary>
    /// The Name property of the element, if it is a FrameworkElement.
    /// </summary>
    public required string Name { get; init; }

    /// <summary>
    /// The child nodes in the visual tree.
    /// </summary>
    public required IReadOnlyList<VisualTreeNodeInfo> Children { get; init; }

    /// <summary>
    /// Returns an indented text representation of the visual tree.
    /// </summary>
    public override string ToString()
    {
        var sb = new System.Text.StringBuilder();
        AppendTo(sb, 0);
        return sb.ToString();
    }

    private void AppendTo(System.Text.StringBuilder sb, int depth)
    {
        sb.Append(' ', depth * 2);
        sb.Append(Type);
        if (!string.IsNullOrEmpty(Name))
        {
            sb.Append($" (Name=\"{Name}\")");
        }
        sb.AppendLine();
        foreach (var child in Children)
        {
            child.AppendTo(sb, depth + 1);
        }
    }
}
