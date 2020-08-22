using System;
using System.Collections.Generic;
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

        Task SendInput(KeyboardInput keyboardInput);
        //Task SendInput(MouseInput mouseInput);

        Task<IImage> GetBitmap();
    }



    public sealed class KeyboardInput
    {
        public string Text { get; }
        public IReadOnlyList<Key> Keys { get; }

        public KeyboardInput(string text)
        {
            Text = text ?? throw new ArgumentNullException(nameof(text));
            Keys = Array.Empty<Key>();
        }

        public KeyboardInput(params Key[] keys)
        {
            Text = "";
            Keys = keys;
        }
    }

    public sealed class MouseInput
    {

    }
}
