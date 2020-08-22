using System;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using static PInvoke.User32;

namespace XamlTest
{
    public static partial class VisualElementMixins
    {
        public static async Task MoveCursorToElement(
            this IVisualElement element, Position position = Position.Center)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            Rect coordinates = await element.GetCoordinates();

            Point location = position switch
            {
                Position.TopLeft => coordinates.TopLeft,
                _ => coordinates.Center()
            };

            SetCursorPos((int)location.X, (int)location.Y);
        }

        public static async Task Click(this IVisualElement element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            await MoveCursorToElement(element);
            LeftClick();
        }

        private static unsafe void LeftClick()
            => mouse_event(mouse_eventFlags.MOUSEEVENTF_LEFTDOWN | mouse_eventFlags.MOUSEEVENTF_LEFTUP, 0, 0, 0, null);

        public static async Task SendInput(this IVisualElement element, string textInput) 
            => await element.SendInput(new KeyboardInput(textInput));

        public static async Task SendInput(this IVisualElement element, params Key[] keys) 
            => await element.SendInput(new KeyboardInput(keys));
    }
}
