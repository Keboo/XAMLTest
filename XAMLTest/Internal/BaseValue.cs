namespace XamlTest.Internal;

internal abstract class BaseValue(string? valueType, object? value, AppContext context) : IValue
{
    protected AppContext Context { get; } = context ?? throw new ArgumentNullException(nameof(context));
    protected Serializer Serializer => Context.Serializer;

    public object? Value { get; } = value;
    public string? ValueType { get; } = valueType;

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
