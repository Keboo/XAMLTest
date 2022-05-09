using System.Diagnostics.CodeAnalysis;

namespace XamlTest;

public interface IValue
{
    object? Value { get; }
    string? ValueType { get; }

    [return: MaybeNull]
    T GetAs<T>();
}
