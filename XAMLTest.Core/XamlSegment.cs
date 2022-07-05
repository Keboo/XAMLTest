using System.Collections.Generic;
using System.Text;

namespace XamlTest;

public sealed class XamlSegment
{
    public string Xaml { get; }
    public IReadOnlyList<XmlNamespace> Namespaces { get; }

    public XamlSegment(string xaml, params XmlNamespace[] namespaces)
    {
        Xaml = xaml;
        Namespaces = namespaces;
    }

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