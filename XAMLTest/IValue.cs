using System.Diagnostics.CodeAnalysis;

namespace XamlTest
{
    public interface IValue
    {
        string Value { get; }
        string? ValueType { get; }

        [return: MaybeNull]
        T GetAs<T>();
    }
}
