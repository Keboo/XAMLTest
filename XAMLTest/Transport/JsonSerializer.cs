﻿using System.Text.Json;

namespace XamlTest.Transport;

public abstract class JsonSerializer<T> : ISerializer
{
    public bool CanSerialize(Type type, ISerializer rootSerializer)
        => type == typeof(T);

    public virtual JsonSerializerOptions? Options { get; }

    public object? Deserialize(Type type, string value, ISerializer rootSerializer)
    {
        if (!string.IsNullOrEmpty(value))
        {
            var rv = JsonSerializer.Deserialize<T>(value, Options);
            return rv;
        }
        return default(T);
    }

    public string Serialize(Type type, object? value, ISerializer rootSerializer)
    {
        if (value is not null)
        {
            var rv = JsonSerializer.Serialize(value, Options);
            return rv;
        }
        return "";
    }
}
