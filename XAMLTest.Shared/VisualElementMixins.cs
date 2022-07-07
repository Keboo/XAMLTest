using System.Threading.Tasks;

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

#if WPF
        IValue value = await element.GetProperty(dependencyProperty.Name, dependencyProperty.OwnerType.AssemblyQualifiedName);
#elif WIN_UI
        IValue value = await element.GetProperty("", "");
#endif
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

#if WPF
        return await SetProperty(element, dependencyProperty.Name, value, dependencyProperty.OwnerType.AssemblyQualifiedName);
#elif WIN_UI
        return await SetProperty(element, "", value, "");
#endif
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
        IValue newValue = await element.SetProperty(propertyName, value?.ToString() ?? "", typeof(T).AssemblyQualifiedName, ownerType);
        if (newValue is { })
        {
            return newValue.GetAs<T?>();
        }
        return default;
    }
}
