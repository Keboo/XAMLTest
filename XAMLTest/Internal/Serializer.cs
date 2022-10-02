using XamlTest.Transport;

namespace XamlTest.Internal;

internal class Serializer : ISerializer
{
    public List<ISerializer> Serializers { get; } = new List<ISerializer>();

    public Serializer()
    {
        //NB: Order matters here. Items earlier in the list take precedence
        Serializers.Add(new XamlSegmentSerializer());
        Serializers.Add(new BrushSerializer());
        Serializers.Add(new DpiScaleSerializer());
        Serializers.Add(new CharSerializer());
        Serializers.Add(new GridSerializer());
        Serializers.Add(new DependencyPropertyConverter());
        Serializers.Add(new SecureStringSerializer());
        Serializers.Add(new DefaultSerializer());
    }

    public void AddSerializer(ISerializer serializer, int index) 
        => Serializers.Insert(index, serializer);

    public string Serialize(Type type, object? value)
        => ((ISerializer)this).Serialize(type, value, this);

    string ISerializer.Serialize(Type type, object? value, ISerializer rootSerializer)
    {
        if (Serializers.FirstOrDefault(x => x.CanSerialize(type, rootSerializer)) is { } serializer)
        {
            return serializer.Serialize(type, value, rootSerializer);
        }
        return "";
    }

    public object? Deserialize(Type type, string value)
        => ((ISerializer)this).Deserialize(type, value, this);

    object? ISerializer.Deserialize(Type type, string value, ISerializer rootSerializer)
    {
        if (Serializers.FirstOrDefault(x => x.CanSerialize(type, rootSerializer)) is { } serializer)
        {
            return serializer.Deserialize(type, value, rootSerializer);
        }
        return null;
    }

    public T? Deserialize<T>(string? value)
    {
        if (value is not null)
        {
            return (T?)((ISerializer)this).Deserialize(typeof(T), value, this);
        }
        return default;
    }

    bool ISerializer.CanSerialize(Type type, ISerializer rootSerializer) 
        => Serializers.Any(x => x.CanSerialize(type, rootSerializer));
}
