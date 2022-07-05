using XamlTest.Transport;

namespace XamlTest.Internal;

public class Serializer
{
    public List<ISerializer> Serializers { get; } = new List<ISerializer>();

    public Serializer()
    {
        //NB: Order matters here. Items earlier in the list take precedence
        Serializers.Add(new XamlSegmentSerializer());
        Serializers.Add(new CharSerializer());
        Serializers.Add(new SecureStringSerializer());
        Serializers.Add(new DefaultSerializer());
    }

    public void AddSerializer(ISerializer serializer, int index = 0)
        => Serializers.Insert(index, serializer);

    public string? Serialize(Type type, object? value)
    {
        if (Serializers.FirstOrDefault(x => x.CanSerialize(type)) is { } serializer)
        {
            return serializer.Serialize(type, value);
        }
        return null;
    }

    public object? Deserialize(Type type, string value)
    {
        if (Serializers.FirstOrDefault(x => x.CanSerialize(type)) is { } serializer)
        {
            return serializer.Deserialize(type, value);
        }
        return null;
    }

    public T? Deserialize<T>(string? value)
    {
        if (value is not null)
        {
            return (T?)Deserialize(typeof(T), value);
        }
        return default;
    }
}
