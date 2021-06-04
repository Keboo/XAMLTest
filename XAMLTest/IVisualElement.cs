using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace XamlTest
{
    public static class FooEx
    {
        public static void Do<T>(this IFoo<T> button)
            where T : Button
        { }

        public static void DoNot<T>(this IFoo<T> fe)
            where T : FrameworkElement
        { }

        public static void Baz()
        {
            IFoo<Button>? x = null;
            x.Do();
            x.DoNot();
        }
    }

    public interface IFoo<T>
    { }

    public interface IVisualElement<TElement> : IVisualElement
    {

    }

    public interface IVisualElement : IEquatable<IVisualElement>
    {
        Task<IVisualElement> GetElement(string query);
        Task<IVisualElement<TElement>> GetElement<TElement>(string query);

        Task<IValue> GetProperty(string name, string? ownerType);
        Task<IValue> SetProperty(string name, string value, string? valueType, string? ownerType);

        Task<IResource> GetResource(string key);

        Task<Color> GetEffectiveBackground(IVisualElement? toElement);
        Task<Rect> GetCoordinates();

        Task<IEventRegistration> RegisterForEvent(string name);
        Task UnregisterEvent(IEventRegistration eventRegistration);

        Task MoveKeyboardFocus();

        Task SendInput(KeyboardInput keyboardInput);
        //Task SendInput(MouseInput mouseInput);
    }
}
