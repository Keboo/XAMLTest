namespace XamlTest.Internal;

internal class AppContext
{
    public Serializer Serializer { get; } = new();

    public List<XmlNamespace> DefaultNamespaces { get; }

    public AppContext()
    {
        DefaultNamespaces = new()
        {
            new XmlNamespace("", "http://schemas.microsoft.com/winfx/2006/xaml/presentation"),
            new XmlNamespace("x", "http://schemas.microsoft.com/winfx/2006/xaml"),
            new XmlNamespace("d", "http://schemas.microsoft.com/expression/blend/2008"),
            new XmlNamespace("mc", "http://schemas.openxmlformats.org/markup-compatibility/2006"),
        };
    }
}
