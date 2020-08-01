using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media;
using static PInvoke.User32;

namespace XamlTest
{
    public static partial class VisualElementMixins
    {
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

        public static async Task<T> GetProperty<T>(this IVisualElement element, string propertyName)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            IValue value = await element.GetProperty(propertyName);
#pragma warning disable CS8603 // Possible null reference return.
            return value.GetValueAs<T>();
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static async Task<string> GetText(this IVisualElement element)
            => await element.GetProperty<string>("Text");

        public static async Task<Color> GetBackgroundColor(this IVisualElement element)
            => await element.GetProperty<Color>("Background");

        public static async Task<Color> GetForegroundColor(this IVisualElement element)
            => await element.GetProperty<Color>("Foreground");

        public static async Task<object> GetContent(this IVisualElement element)
            => await element.GetProperty<object>("Content");

        public static async Task<T> SetProperty<T>(this IVisualElement element, string propertyName, T value)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            if (string.IsNullOrEmpty(propertyName))
            {
                throw new ArgumentException($"'{nameof(propertyName)}' cannot be null or empty", nameof(propertyName));
            }

            IValue newValue = await element.SetProperty(propertyName, value?.ToString() ?? "", typeof(T).AssemblyQualifiedName);
            if (newValue is { })
            {
#pragma warning disable CS8603 // Possible null reference return.
                return newValue.GetValueAs<T>();
#pragma warning restore CS8603 // Possible null reference return.
            }
#pragma warning disable CS8603 // Possible null reference return.
            return default;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public static async Task<Color> SetBackgroundColor(this IVisualElement element, Color color)
        {
            SolidColorBrush? brush = await element.SetProperty("Background", new SolidColorBrush(color));
            return brush?.Color ?? default;
        }

        public static async Task<string> SetText(this IVisualElement element, string text)
            => await element.SetProperty<string>("Text", text);
    }
}
