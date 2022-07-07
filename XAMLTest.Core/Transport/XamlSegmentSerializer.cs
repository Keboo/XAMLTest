using System.Text.Json;

namespace XamlTest.Transport;

public class XamlSegmentSerializer : ISerializer
{
    public bool CanSerialize(Type type)
        => type == typeof(XamlSegment);

    public object? Deserialize(Type type, string value)
    {
        if (type == typeof(XamlSegment))
        {
            return JsonSerializer.Deserialize<XamlSegment>(value);
        }
        return null;
    }

    public string Serialize(Type type, object? value)
    {
        return value switch
        {
            XamlSegment segment => JsonSerializer.Serialize(segment),
            _ => ""
        };
    }
}
