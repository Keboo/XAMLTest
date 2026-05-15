namespace XAMLTest.Mcp;

public static class SharedStrings
{
    public const string AppIdDescription = "The XAMLTest application id";

    public const string XamlSnippetDescription =
        """
        The XAML snippet to render in a WPF Window.
        This should only include the content inside the Window tags.
        """;

    public const string ElementQueryDescription =
        """
        A query string to find a visual element. Supported formats:
        ~<Name> - Search by element Name (default if no prefix).
        /<TypeName> - Search by type name (e.g. /Button). Supports base types and [Index] suffix (e.g. /Button[1]).
        .<PropertyName> - Navigate to a property value of the previous element.
        [<Property>=<Value>] - Search for an element where the property matches the value.
        Queries can be chained (e.g. /StackPanel/Button[0]).
        """;

    public const string PropertyNameDescription = "The name of the property to access (e.g. Content, Text, IsEnabled, Width).";

    public const string InputActionsJsonDescription =
        """
        A JSON array of ordered interaction actions to execute on the target element.
        Each action must include a `type` property. Supported action types:
        - focus
        - delay (milliseconds)
        - mouse_move_to_element (position, xOffset, yOffset)
        - mouse_move_relative (xOffset, yOffset)
        - mouse_move_absolute (x, y)
        - mouse_button_down (button: left|right|middle)
        - mouse_button_up (button: left|right|middle)
        - mouse_click (button, count, position, xOffset, yOffset, clickDelayMs)
        - keyboard_text (text)
        - keyboard_keys (keys: string[])
        Example:
        [
          { "type": "focus" },
          { "type": "mouse_click", "button": "left", "position": "Center" },
          { "type": "keyboard_text", "text": "Hello world" },
          { "type": "keyboard_keys", "keys": ["Enter"] }
        ]
        """;
}
