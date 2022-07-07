namespace XamlTest.Host;

partial class InternalTestService 
{
    protected abstract string? Serialize(Type type, object? value);

    protected abstract void AddSerializer(ISerializer serializer, int index = 0);
}
