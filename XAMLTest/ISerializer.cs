namespace XamlTest;

public interface ISerializer
{
    bool CanSerialize(Type type, ISerializer rootSerializer);
    string Serialize(Type type, object? value, ISerializer rootSerializer);
    object? Deserialize(Type type, string value, ISerializer rootSerializer);
}
