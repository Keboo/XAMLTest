using System;
using System.Diagnostics.CodeAnalysis;

namespace XamlTest.Internal;

internal abstract class BaseValue : IValue
{
    protected Serializer Serializer { get; }

    public object? Value { get; }
    public string? ValueType { get; }

    protected BaseValue(string? valueType, object? value, Serializer serializer)
    {
        ValueType = valueType;
        Value = value;
        Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
    }

    [return: MaybeNull]
    public virtual T GetAs<T>()
    {
        if (ValueType is null)
        {
            return default;
        }

        if (Value is T converted && typeof(T) != typeof(string))
        {
            return converted;
        }

        return (T)Serializer.Deserialize(typeof(T), Value?.ToString() ?? "")!;
    }
}
