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
        public static async Task<Point> MoveCurosrTo(this IVisualElement element,
            Position position = Position.Center,
            int xOffset = 0,
            int yOffset = 0)
        {
            List<MouseInput> inputs = new();
            inputs.Add(MouseInput.MoveToElement(position));
            if (xOffset != 0 || yOffset != 0)
            {
                inputs.Add(MouseInput.MoveRelative(xOffset, yOffset));
            }
            return await element.SendInput(new MouseInput(inputs.ToArray()));
        }

        public static async Task<Point> LeftClick(this IVisualElement element,
            Position position = Position.Center,
            TimeSpan? clickTime = null)
        {
            return await SendClick(element,
                MouseInput.LeftDown(),
                MouseInput.LeftUp(),
                position,
                clickTime);
        }

        public static async Task<Point> RightClick(this IVisualElement element,
            Position position = Position.Center,
            TimeSpan? clickTime = null)
        {
            return await SendClick(element,
                MouseInput.RightDown(),
                MouseInput.RightUp(),
                position,
                clickTime);
        }

        public static async Task<Point> SendClick(IVisualElement element,
            MouseInput down,
            MouseInput up,
            Position position,
            TimeSpan? clickTime)
        {
            List<MouseInput> inputs = new();
            inputs.Add(MouseInput.MoveToElement(position));
            inputs.Add(down);
            if (clickTime != null)
            {
                inputs.Add(MouseInput.Delay(clickTime.Value));
            }
            inputs.Add(up);
            
            return await element.SendInput(new MouseInput(inputs.ToArray()));
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
