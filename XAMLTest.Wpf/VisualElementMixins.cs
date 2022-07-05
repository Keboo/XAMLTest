namespace XamlTest;

public static partial class VisualElementMixins
{
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

        return await VisualElementMixins.SetProperty(element, dependencyProperty.Name, value, dependencyProperty.OwnerType.AssemblyQualifiedName);
    }
}
