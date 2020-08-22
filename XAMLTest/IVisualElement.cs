using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;

namespace XamlTest
{
    public interface IVisualElement : IEquatable<IVisualElement>
    {
        Task<IVisualElement> GetElement(string query);
        
        Task<IValue> GetProperty(string name, string? ownerType);
        Task<IValue> SetProperty(string name, string value, string? valueType, string? ownerType);

        Task<IResource> GetResource(string key);

        Task<Color> GetEffectiveBackground(IVisualElement? toElement);
        Task<Rect> GetCoordinates();

        Task MoveKeyboardFocus();
        Task SendInput(string textInput);
        Task SendInput(params Key[] keys);

        Task<IImage> GetBitmap();
    }

    public interface IVisualElement<TElement> : IVisualElement
    {
        Task<T> Get<T>(Expression<Func<TElement, T>> propertyExpression);
    }
}
