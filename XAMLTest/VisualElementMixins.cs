using System;
using System.Globalization;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
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

    public static Task SetValidationError<T>(this IVisualElement<T> element, DependencyProperty property, object errorContent)
        where T : DependencyObject
    {
        return element.RemoteExecute(SetError, property, errorContent);

        static void SetError(T element, DependencyProperty property, object errorContent)
        {
            BindingExpressionBase? bindingExpression = BindingOperations.GetBindingExpression(element, property);
            if (bindingExpression is null)
            {
                Binding binding = new()
                {
                    Path = new PropertyPath(Internal.Validation.ErrorProperty),
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self)
                };
                bindingExpression = BindingOperations.SetBinding(element, property, binding);
            }
            ValidationError validationError = new(new Internal.Validation.Rule(errorContent), bindingExpression)
            {
                ErrorContent = errorContent
            };
            System.Windows.Controls.Validation.MarkInvalid(bindingExpression, validationError);
        }
    }

    public static Task ClearValidationError<T>(this IVisualElement<T> element, DependencyProperty property)
        where T : DependencyObject
    {
        return element.RemoteExecute(ClearInvalid, property);

        static void ClearInvalid(T element, DependencyProperty property)
        {
            BindingExpressionBase? bindingExpression = BindingOperations.GetBindingExpression(element, property);
            if (bindingExpression != null)
            {
                // Clear the invalidation
                System.Windows.Controls.Validation.ClearInvalid(bindingExpression);
            }
        }
    }

    public static Task<TErrorContext?> GetValidationError<T, TErrorContext>(this IVisualElement<T> element, DependencyProperty dependencyProperty)
        where T : DependencyObject
    {
        if (element is null)
        {
            throw new ArgumentNullException(nameof(element));
        }

        return element.RemoteExecute(GetValidationErrorContent, dependencyProperty);

        static TErrorContext? GetValidationErrorContent(T element, DependencyProperty property)
        {
            var errors = System.Windows.Controls.Validation.GetErrors(element);
            foreach (var error in errors)
            {
                if (error.BindingInError is BindingExpressionBase bindingExpressionBase &&
                    bindingExpressionBase.TargetProperty == property)
                {
                    if (error.ErrorContent is TErrorContext converted)
                    {
                        return converted;
                    }
                }
            }
            return default;
        }
    }
}
