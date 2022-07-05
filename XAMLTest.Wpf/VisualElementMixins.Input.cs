using System.Windows.Input;
using XamlTest.Input;

namespace XamlTest;

public static partial class VisualElementMixins
{
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
