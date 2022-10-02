using System.ComponentModel;

namespace XamlTest.Transport;

public class DefaultSerializer : ISerializer
{
    public virtual bool CanSerialize(Type _, ISerializer rootSerializer) => true;

    public virtual string Serialize(Type type, object? value, ISerializer rootSerializer)
    {
        if (value is null) return "";
        var converter = TypeDescriptor.GetConverter(type);
        return converter.ConvertToInvariantString(value) ?? "";
    }

    public virtual object? Deserialize(Type type, string value, ISerializer rootSerializer)
    {
        var converter = TypeDescriptor.GetConverter(type);
        if (converter.CanConvertFrom(typeof(string)))
        {
            if (string.IsNullOrEmpty(value))
            {
                return null;
            }
            return converter.ConvertFromInvariantString(value);
        }
        return value;
    }
}
