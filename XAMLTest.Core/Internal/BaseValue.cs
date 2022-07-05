using System;
using System.Diagnostics.CodeAnalysis;

namespace XamlTest.Internal;

internal abstract class BaseValue : IValue
{
    protected AppContext Context { get; }
    protected Serializer Serializer => Context.Serializer;

    public object? Value { get; }
    public string? ValueType { get; }

    protected BaseValue(string? valueType, object? value, AppContext context)
    {
        ValueType = valueType;
        Value = value;
        Context = context ?? throw new ArgumentNullException(nameof(context));
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
