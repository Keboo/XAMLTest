using System;
using System.Diagnostics.CodeAnalysis;

namespace XamlTest.Internal
{
    internal abstract class BaseValue : IValue
    {
        protected Serializer Serializer { get; }

        public string Value { get; }
        public string? ValueType { get; }

        protected BaseValue(string? valueType, string value, Serializer serializer)
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

            return (T)Serializer.Deserialize(typeof(T), Value);
        }
    }
}
