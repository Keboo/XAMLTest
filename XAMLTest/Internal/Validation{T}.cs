using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace XamlTest.Internal;

internal class Validation<T> : IValidation<T>
    where T : DependencyObject
{
    private IVisualElement<T> Element { get; }

    public Validation(IVisualElement<T> element)
    {
        Element = element ?? throw new ArgumentNullException(nameof(element));
    }

    public Task SetValidationError(DependencyProperty property, object errorContent)
    {
        return Element.RemoteExecute(SetError, property, errorContent);

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

    public Task ClearValidationError(DependencyProperty property)
    {
        return Element.RemoteExecute(ClearInvalid, property);

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

    public Task<TErrorContext?> GetValidationError<TErrorContext>(DependencyProperty dependencyProperty)
    {
        return Element.RemoteExecute(GetValidationErrorContent, dependencyProperty);

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
