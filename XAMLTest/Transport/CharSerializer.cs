namespace XamlTest.Transport;

public class CharSerializer : ISerializer
{
    public bool CanSerialize(Type type, ISerializer rootSerializer)
        => type == typeof(char) ||
           type == typeof(char?);

    public object? Deserialize(Type type, string value, ISerializer rootSerializer)
    {
        if (type == typeof(char))
        {
            if (value?.Length == 1)
            {
                return value[0];
            }
        }
        else if (type == typeof(char?))
        {
            if (string.IsNullOrEmpty(value) ||
                value.Length != 1)
            {
                return null;
            }
            return value[0];
        }
        return '\0';
    }

    public string Serialize(Type type, object? value, ISerializer rootSerializer)
    {
        return value switch
        {
            char c => c.ToString(),
            _ => ""
        };
    }
}
