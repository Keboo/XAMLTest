using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace XamlTest;

public interface IValidation<T> 
    where T : DependencyObject
{
    Task<bool> GetHasError();
    Task SetValidationError(DependencyProperty property, object errorContent);
    Task SetValidationRule<TRule>(DependencyProperty property)
        where TRule : ValidationRule, new();
    Task<TErrorContext?> GetValidationError<TErrorContext>(DependencyProperty dependencyProperty);
    Task ClearValidationError(DependencyProperty property);
}
