using XamlTest.Internal;

namespace XamlTest.Host;

partial class TestService 
{
    private Serializer Serializer { get; } = new();

    protected override string? Serialize(Type type, object? value)
        => Serializer.Serialize(type, value);

    protected override void AddSerializer(ISerializer serializer, int index = 0)
        => Serializer.AddSerializer(serializer, index);
}
