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

            MouseInput.MoveCursor(location);
        }

        public static async Task Click(this IVisualElement element)
        {
            if (element is null)
            {
                throw new ArgumentNullException(nameof(element));
            }

            await MoveCursorToElement(element);
            MouseInput.LeftClick();
        }

        public static async Task SendInput(this IVisualElement element, FormattableString input)
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
