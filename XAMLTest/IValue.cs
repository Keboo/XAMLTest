﻿namespace XamlTest
{
    public interface IValue
    {
        string Value { get; }
        string? ValueType { get; }

        T GetAs<T>();
    }
}
