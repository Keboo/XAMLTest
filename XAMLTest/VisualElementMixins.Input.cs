using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using XamlTest.Input;

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

            Input.MouseInput.MoveCursor(location);
        }

        public static async Task LeftClick(this IVisualElement element)
        {
            await SendMouseInput(element, new[]
            {
                MouseInput.LeftDown(),
                MouseInput.LeftUp()
            });
        }

        public static async Task RightClick(this IVisualElement element)
        {
            await SendMouseInput(element, new[]
            {
                MouseInput.RightDown(),
                MouseInput.RightUp()
            });
        }

        public static async Task SendMouseInput(this IVisualElement element,
            params MouseInput[] mouseInputs)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            var moveToElement = MouseInput.MoveToElement(element);
            
            await element.SendInput(new MouseInput(
                new IInput[] { moveToElement }.Concat(mouseInputs).ToArray()));
        }

        public static async Task SendKeyboardInput(this IVisualElement element, FormattableString input)
        {
            var placeholder = Guid.NewGuid().ToString("N");
            string formatted = string.Format(input.Format, Enumerable.Repeat(placeholder, input.ArgumentCount).Cast<object>().ToArray());
            string[] textParts = formatted.Split(placeholder);

            var inputs = new List<IInput>();
            int argumentIndex = 0;
            foreach (string? part in textParts)
            {
                if (!string.IsNullOrEmpty(part))
                {
                    inputs.Add(new TextInput(part));
                }
                if (argumentIndex < input.ArgumentCount)
                {
                    object? argument = input.GetArgument(argumentIndex++);
                    switch (argument)
                    {
                        case Key key:
                            inputs.Add(new KeysInput(key));
                            break;
                        case IEnumerable<Key> keys:
                            inputs.Add(new KeysInput(keys));
                            break;
                        default:
                            string? stringArgument = argument?.ToString();
                            if (!string.IsNullOrEmpty(stringArgument))
                            {
                                inputs.Add(new TextInput(stringArgument));
                            }
                            break;
                    }
                }
            }
            await element.SendInput(new KeyboardInput(inputs.ToArray()));
        }
    }
}
