namespace XamlTest;

public sealed class XamlSegment(string xaml, params XmlNamespace[] namespaces)
{
    public string Xaml { get; } = xaml;
    public IReadOnlyList<XmlNamespace> Namespaces { get; } = namespaces;

    public static implicit operator XamlSegment(string xamlString) => new(xamlString);

    public override string ToString()
    {
        StringBuilder sb = new();
        foreach(var @namespace in Namespaces)
        {
            sb.AppendLine(@namespace.ToString());
        }
        sb.Append(Xaml);
        return sb.ToString();
    }
}