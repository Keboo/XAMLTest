namespace XamlTest;

public static class XmlNamespaceMixins
{
    public static void Add(this IList<XmlNamespace> list, string? prefix, string uri)
        => list.Add(new XmlNamespace(prefix, uri));
}
