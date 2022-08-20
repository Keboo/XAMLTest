using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using XamlTest.Internal;

namespace XamlTest;

public static partial class VisualElementMixins
{
    public static async Task<Color> GetEffectiveBackground(this IVisualElement element)
        => await element.GetEffectiveBackground(null);

    public static async Task<IValue> GetProperty(this IVisualElement element, string name)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return await element.GetProperty(name, null);
    }

    public static async Task<T?> GetProperty<T>(this IVisualElement element, string propertyName)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        IValue value = await element.GetProperty(propertyName);

        return value.GetAs<T?>();
    }

    public static async Task<T?> GetProperty<T>(this IVisualElement element, DependencyProperty dependencyProperty)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        if (dependencyProperty is null)
        {
            throw new ArgumentNullException(nameof(dependencyProperty));
        }

        IValue value = await element.GetProperty(dependencyProperty.Name, dependencyProperty.OwnerType.AssemblyQualifiedName);
        return value.GetAs<T?>();
    }

    public static async Task<IValue> SetProperty(this IVisualElement element,
        string name, string value, string? valueType = null)
    {
        return await element.SetProperty(name, value, valueType, null);
    }

    public static async Task<T?> SetProperty<T>(this IVisualElement element, DependencyProperty dependencyProperty, T value)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        if (dependencyProperty is null)
        {
            throw new ArgumentNullException(nameof(dependencyProperty));
        }

        return await SetProperty(element, dependencyProperty.Name, value, dependencyProperty.OwnerType.AssemblyQualifiedName);
    }

    public static async Task<T?> SetProperty<T>(this IVisualElement element, string propertyName, T value)
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        if (string.IsNullOrEmpty(propertyName))
        {
            throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty", nameof(propertyName));
        }

        return await SetProperty(element, propertyName, value, null);
    }

    private static async Task<T?> SetProperty<T>(IVisualElement element, string propertyName, T value, string? ownerType)
    {
        IValue newValue = await element.SetProperty(propertyName, (value != null ? Convert.ToString(value, CultureInfo.InvariantCulture) : "") ?? "", typeof(T).AssemblyQualifiedName, ownerType);
        if (newValue is { })
        {
            return newValue.GetAs<T?>();
        }
        return default;
    }

    public static IValidation<T> Validation<T>(this IVisualElement<T> element)
        where T : DependencyObject
    {
        return new Validation<T>(element);
    }

    public static Task<TResult?> RemoteExecute<T, T1, TResult>(this IVisualElement<T> element,
        Func<T, T1, TResult> action, T1 param1)
    {
        return element.RemoteExecute<TResult>(action, new object?[] { param1 });
    }

    public static Task RemoteExecute<T, T1>(this IVisualElement<T> element,
        Action<T, T1> action, T1 param1)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1 });
    }

    public static Task RemoteExecute<T, T1, T2>(this IVisualElement<T> element, 
        Action<T, T1, T2> action, T1 param1, T2 param2)
    {
        return element.RemoteExecute<object?>(action, new object?[] { param1, param2 });
    }
}
