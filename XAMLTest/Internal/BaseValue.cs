using System;
using System.Diagnostics.CodeAnalysis;

namespace XamlTest.Internal
{
    internal abstract class BaseValue : IValue
    {
        public static string VisualElementType =
            typeof(IVisualElement<>).AssemblyQualifiedName!;

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
        public T GetAs<T>()
        {
            if (ValueType is null)
            {
                return default;
            }

            if (Value is T converted && typeof(T) != typeof(string))
            {
                return converted;
            }
            Type desiredType = typeof(T);
            if (ValueType == VisualElementType &&
                Value is IVisualElementConverter converter)
            {
                return converter.Convert<T>();
            }

            return (T)Serializer.Deserialize(desiredType, Value?.ToString() ?? "")!;
        }
    }
}
