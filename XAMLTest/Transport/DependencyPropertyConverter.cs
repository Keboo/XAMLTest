﻿namespace XamlTest.Transport;

public class DependencyPropertyConverter : ISerializer
{
    public bool CanSerialize(Type type, ISerializer rootSerializer) => type == typeof(DependencyProperty);
    
    public object? Deserialize(Type type, string value, ISerializer rootSerializer)
    {
        if (type == typeof(DependencyProperty) && !string.IsNullOrEmpty(value))
        {
            if (System.Text.Json.JsonSerializer.Deserialize<DependencyPropertyData>(value) is { } data &&
                DependencyPropertyHelper.TryGetDependencyProperty(data.Name!, data.OwnerType!,
                out DependencyProperty? dependencyProperty))
            {
                return dependencyProperty;
            }
        }
        return null;
    }

    public string Serialize(Type type, object? value, ISerializer rootSerializer)
    {
        if (value is DependencyProperty dp)
        {
            return System.Text.Json.JsonSerializer.Serialize(new DependencyPropertyData
            {
                OwnerType = dp.OwnerType.AssemblyQualifiedName,
                Name = dp.Name
            });
        }
        return "";
    }

    private class DependencyPropertyData
    {
        public string? OwnerType { get; set; }
        public string? Name { get; set; }
    }
}
