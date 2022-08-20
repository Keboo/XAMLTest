using System.Threading.Tasks;
using System.Windows;
namespace XamlTest;

public interface IValidation<T> 
    where T : DependencyObject
{
    Task SetValidationError(DependencyProperty property, object errorContent);
    Task<TErrorContext?> GetValidationError<TErrorContext>(DependencyProperty dependencyProperty);
    Task ClearValidationError(DependencyProperty property);
}
