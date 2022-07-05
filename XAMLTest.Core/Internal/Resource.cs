using System;

namespace XamlTest.Internal;

internal class Resource : BaseValue, IResource
{
    public string Key { get; }

    public Resource(string key, string valueType, object? value, AppContext context)
        : base(valueType, value, context)
    {
        Key = key ?? throw new ArgumentNullException(nameof(key));
    }
}
