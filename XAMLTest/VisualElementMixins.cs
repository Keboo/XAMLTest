using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using XamlTest;

namespace XamlTest
{

    public static partial class VisualElementMixins
    {
        public static async Task<Color> GetEffectiveBackground(this IVisualElement element) 
            => await element.GetEffectiveBackground(null);

        public static async Task<IVisualElement?> SetXamlContent(this IVisualElement containerElement, string xaml)
        {
            if (containerElement is null)
            {
                throw new ArgumentNullException(nameof(containerElement));
            }

            if (await containerElement.SetProperty("Content", xaml, Types.XamlString) is { })
            {
                return await containerElement.GetElement(".Content");
            }
            return null;
        }

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

        public static async Task<string?> GetText(this IVisualElement element)
            => await element.GetProperty<string?>("Text");

        public static async Task<Color> GetBackgroundColor(this IVisualElement element)
            => await element.GetProperty<Color>("Background");

        public static async Task<Color> GetForegroundColor(this IVisualElement element)
            => await element.GetProperty<Color>("Foreground");

        public static async Task<object?> GetContent(this IVisualElement element)
            => await element.GetProperty<object?>("Content");

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
            IValue newValue = await element.SetProperty(propertyName, value?.ToString() ?? "", typeof(T).AssemblyQualifiedName, ownerType);
            if (newValue is { })
            {
                return newValue.GetAs<T?>();
            }
            return default;
        }

        public static async Task<Color?> SetBackgroundColor(this IVisualElement element, Color color)
        {
            SolidColorBrush? brush = await element.SetProperty("Background", new SolidColorBrush(color));
            return brush?.Color ?? default;
        }

        public static async Task<string?> SetText(this IVisualElement element, string text)
            => await element.SetProperty("Text", text);
    }
}
