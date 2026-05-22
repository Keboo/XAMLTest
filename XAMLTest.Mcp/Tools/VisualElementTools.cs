using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using System.Text.Json;
using System.Windows.Input;
using XamlTest;
using XAMLTest.Mcp;

[McpServerToolType]
internal class VisualElementTools(AppServiceManager appServiceManager)
    : BaseTools
{
    [McpServerTool]
    [Description("""
        Gets the visual tree of the main window in a running WPF application.
        Returns an indented text representation showing element types, names, and hierarchy.
        """)]
    public async Task<CallToolResult> GetVisualTree(
        [Description(SharedStrings.AppIdDescription)] string appId)
    {
        if (!appServiceManager.TryGetApp(appId, out var app))
        {
            return Failure($"App with id '{appId}' is not running");
        }

        var window = await app.GetMainWindow();
        if (window is null)
        {
            return Failure("Could not get main window");
        }

        var tree = await window.GetVisualTree();
        return Success(tree.ToString());
    }

    [McpServerTool]
    [Description("""
        Gets the value of a property from a visual element in a running WPF application.
        Use GetVisualTree first to discover element names and types, then query by name or type.
        """)]
    public async Task<CallToolResult> GetElementProperty(
        [Description(SharedStrings.AppIdDescription)] string appId,
        [Description(SharedStrings.ElementQueryDescription)] string elementQuery,
        [Description(SharedStrings.PropertyNameDescription)] string propertyName,
        [Description("The assembly-qualified name of the type that owns the property. Usually not needed for standard properties.")]
        string? ownerType = null)
    {
        if (!appServiceManager.TryGetApp(appId, out var app))
        {
            return Failure($"App with id '{appId}' is not running");
        }

        var window = await app.GetMainWindow();
        if (window is null)
        {
            return Failure("Could not get main window");
        }

        var element = await window.FindElement(elementQuery);
        if (element is null)
        {
            return Failure($"Could not find element matching query '{elementQuery}'");
        }

        var value = await element.GetProperty(propertyName, ownerType);
        string result = $"Property: {propertyName}\nValueType: {value.ValueType}\nValue: {value.Value}";
        return Success(result);
    }

    [McpServerTool]
    [Description("""
        Sets the value of a property on a visual element in a running WPF application.
        Use GetVisualTree first to discover element names and types, then query by name or type.
        """)]
    public async Task<CallToolResult> SetElementProperty(
        [Description(SharedStrings.AppIdDescription)] string appId,
        [Description(SharedStrings.ElementQueryDescription)] string elementQuery,
        [Description(SharedStrings.PropertyNameDescription)] string propertyName,
        [Description("The string representation of the value to set.")] string value,
        [Description("The assembly-qualified type name of the value (e.g. 'System.String'). If omitted, the type will be inferred.")]
        string? valueType = null,
        [Description("The assembly-qualified name of the type that owns the property. Usually not needed for standard properties.")]
        string? ownerType = null)
    {
        if (!appServiceManager.TryGetApp(appId, out var app))
        {
            return Failure($"App with id '{appId}' is not running");
        }

        var window = await app.GetMainWindow();
        if (window is null)
        {
            return Failure("Could not get main window");
        }

        var element = await window.FindElement(elementQuery);
        if (element is null)
        {
            return Failure($"Could not find element matching query '{elementQuery}'");
        }

        var result = await element.SetProperty(propertyName, value, valueType, ownerType);
        string response = $"Property: {propertyName}\nValueType: {result.ValueType}\nValue: {result.Value}";
        return Success(response);
    }

    [McpServerTool]
    [Description("""
        Executes ordered direct UI interactions (mouse and keyboard) against a visual element in a running WPF application.
        Use this single tool for mixed interaction flows such as click + typing + key presses.
        """)]
    public async Task<CallToolResult> Interact(
        [Description(SharedStrings.AppIdDescription)] string appId,
        [Description("""
            The query used to select the target element.
            If omitted, the main window is used as the interaction target.
            """)]
        string? elementQuery,
        [Description(SharedStrings.InputActionsJsonDescription)] string inputActionsJson)
    {
        if (!appServiceManager.TryGetApp(appId, out var app))
        {
            return Failure($"App with id '{appId}' is not running");
        }

        var window = await app.GetMainWindow();
        if (window is null)
        {
            return Failure("Could not get main window");
        }

        IVisualElement targetElement = window;
        if (!string.IsNullOrWhiteSpace(elementQuery))
        {
            var foundElement = await window.FindElement(elementQuery);
            if (foundElement is null)
            {
                return Failure($"Could not find element matching query '{elementQuery}'");
            }
            targetElement = foundElement;
        }

        JsonDocument actionsDocument;
        try
        {
            actionsDocument = JsonDocument.Parse(inputActionsJson);
        }
        catch (JsonException ex)
        {
            return Failure($"Invalid JSON payload for inputActionsJson: {ex.Message}");
        }

        using (actionsDocument)
        {
            if (actionsDocument.RootElement.ValueKind != JsonValueKind.Array)
            {
                return Failure("inputActionsJson must be a JSON array of action objects.");
            }

            int executedActions = 0;
            foreach (var (action, index) in actionsDocument.RootElement.EnumerateArray().Select((value, idx) => (value, idx)))
            {
                if (action.ValueKind != JsonValueKind.Object)
                {
                    return Failure($"Action at index {index} must be a JSON object.");
                }

                try
                {
                    await ExecuteAction(action, index, targetElement);
                }
                catch (InvalidOperationException ex)
                {
                    return Failure(ex.Message);
                }

                executedActions++;
            }

            return Success($"Executed {executedActions} interaction action(s).");
        }
    }

    [McpServerTool]
    [Description("""
        Updates the XAML content of a running WPF application with the provided XAML snippet.
        """)]
    public async Task<CallToolResult> UpdateAppXaml(
        [Description(SharedStrings.AppIdDescription)] string appId,
        [Description(SharedStrings.XamlSnippetDescription)] string xamlSnippet)
    {
        if (!appServiceManager.TryGetApp(appId, out var existingApp))
        {
            return Failure($"App with id '{appId}' is not running");
        }
        
        var window = await existingApp.GetMainWindow();
        if (window is null)
        {
            return Failure("Could not get main window");
        }
        
        await window.SetXamlContent(xamlSnippet);
        return Success("XAML content updated successfully");
    }

    private static async Task ExecuteAction(JsonElement action, int index, IVisualElement targetElement)
    {
        string actionType = GetRequiredString(action, "type", index).ToLowerInvariant();
        switch (actionType)
        {
            case "focus":
                await targetElement.MoveKeyboardFocus();
                return;
            case "delay":
            {
                int milliseconds = GetRequiredInt(action, "milliseconds", index);
                if (milliseconds < 0)
                {
                    throw new InvalidOperationException($"Action at index {index} has invalid 'milliseconds'. Expected a non-negative integer.");
                }

                await Task.Delay(milliseconds);
                return;
            }
            case "mouse_move_to_element":
            {
                Position position = GetOptionalPosition(action, "position", Position.Center, index);
                int xOffset = GetOptionalInt(action, "xOffset", 0, index);
                int yOffset = GetOptionalInt(action, "yOffset", 0, index);
                _ = await targetElement.MoveCursorTo(position, xOffset, yOffset);
                return;
            }
            case "mouse_move_relative":
            {
                int xOffset = GetRequiredInt(action, "xOffset", index);
                int yOffset = GetRequiredInt(action, "yOffset", index);
                _ = await targetElement.SendInput(MouseInput.MoveRelative(xOffset, yOffset));
                return;
            }
            case "mouse_move_absolute":
            {
                int x = GetRequiredInt(action, "x", index);
                int y = GetRequiredInt(action, "y", index);
                _ = await targetElement.SendInput(MouseInput.MoveAbsolute(x, y));
                return;
            }
            case "mouse_button_down":
            {
                var button = GetOptionalMouseButton(action, "button", "left", index);
                await targetElement.SendInput(GetButtonDown(button));
                return;
            }
            case "mouse_button_up":
            {
                var button = GetOptionalMouseButton(action, "button", "left", index);
                await targetElement.SendInput(GetButtonUp(button));
                return;
            }
            case "mouse_click":
            {
                var button = GetOptionalMouseButton(action, "button", "left", index);
                int count = GetOptionalInt(action, "count", 1, index);
                if (count <= 0)
                {
                    throw new InvalidOperationException($"Action at index {index} has invalid 'count'. Expected an integer > 0.");
                }

                Position position = GetOptionalPosition(action, "position", Position.Center, index);
                int xOffset = GetOptionalInt(action, "xOffset", 0, index);
                int yOffset = GetOptionalInt(action, "yOffset", 0, index);
                int? clickDelayMs = GetOptionalNullableInt(action, "clickDelayMs", index);
                if (clickDelayMs is < 0)
                {
                    throw new InvalidOperationException($"Action at index {index} has invalid 'clickDelayMs'. Expected a non-negative integer.");
                }

                _ = await targetElement.MoveCursorTo(position, xOffset, yOffset);
                for (int click = 0; click < count; click++)
                {
                    await targetElement.SendInput(GetButtonDown(button));
                    if (clickDelayMs is int delayMs and > 0)
                    {
                        await Task.Delay(delayMs);
                    }
                    await targetElement.SendInput(GetButtonUp(button));
                }
                return;
            }
            case "keyboard_text":
            {
                string text = GetRequiredString(action, "text", index);
                await targetElement.SendInput(new KeyboardInput(text));
                return;
            }
            case "keyboard_keys":
            {
                var keys = GetRequiredKeys(action, "keys", index);
                await targetElement.SendInput(new KeyboardInput(keys));
                return;
            }
            default:
                throw new InvalidOperationException(
                    $"Action at index {index} has unsupported type '{actionType}'. " +
                    "Supported types: focus, delay, mouse_move_to_element, mouse_move_relative, mouse_move_absolute, " +
                    "mouse_button_down, mouse_button_up, mouse_click, keyboard_text, keyboard_keys.");
        }
    }

    private static string GetRequiredString(JsonElement action, string propertyName, int index)
    {
        if (!action.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException(
                $"Action at index {index} is missing required string property '{propertyName}'.");
        }

        string? value = property.GetString();
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                $"Action at index {index} has an empty value for '{propertyName}'.");
        }

        return value;
    }

    private static int GetRequiredInt(JsonElement action, string propertyName, int index)
    {
        if (!action.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out int value))
        {
            throw new InvalidOperationException(
                $"Action at index {index} is missing required integer property '{propertyName}'.");
        }

        return value;
    }

    private static int GetOptionalInt(JsonElement action, string propertyName, int defaultValue, int index)
    {
        if (!action.TryGetProperty(propertyName, out var property))
        {
            return defaultValue;
        }

        if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out int value))
        {
            throw new InvalidOperationException(
                $"Action at index {index} has invalid integer property '{propertyName}'.");
        }

        return value;
    }

    private static int? GetOptionalNullableInt(JsonElement action, string propertyName, int index)
    {
        if (!action.TryGetProperty(propertyName, out var property))
        {
            return null;
        }

        if (property.ValueKind == JsonValueKind.Null)
        {
            return null;
        }

        if (property.ValueKind != JsonValueKind.Number || !property.TryGetInt32(out int value))
        {
            throw new InvalidOperationException(
                $"Action at index {index} has invalid integer property '{propertyName}'.");
        }

        return value;
    }

    private static Position GetOptionalPosition(JsonElement action, string propertyName, Position defaultPosition, int index)
    {
        if (!action.TryGetProperty(propertyName, out var property))
        {
            return defaultPosition;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException(
                $"Action at index {index} has invalid '{propertyName}'. Expected a string Position value.");
        }

        string? rawPosition = property.GetString();
        if (string.IsNullOrWhiteSpace(rawPosition) || !Enum.TryParse(rawPosition, true, out Position parsedPosition))
        {
            throw new InvalidOperationException(
                $"Action at index {index} has invalid '{propertyName}' value '{rawPosition}'.");
        }

        return parsedPosition;
    }

    private static string GetOptionalMouseButton(JsonElement action, string propertyName, string defaultButton, int index)
    {
        if (!action.TryGetProperty(propertyName, out var property))
        {
            return defaultButton;
        }

        if (property.ValueKind != JsonValueKind.String)
        {
            throw new InvalidOperationException(
                $"Action at index {index} has invalid '{propertyName}'. Expected one of left, right, or middle.");
        }

        string? rawButton = property.GetString();
        return rawButton?.ToLowerInvariant() switch
        {
            "left" => "left",
            "right" => "right",
            "middle" => "middle",
            _ => throw new InvalidOperationException(
                $"Action at index {index} has invalid '{propertyName}' value '{rawButton}'. Expected left, right, or middle.")
        };
    }

    private static MouseInput GetButtonDown(string button) => button switch
    {
        "left" => MouseInput.LeftDown(),
        "right" => MouseInput.RightDown(),
        "middle" => MouseInput.MiddleDown(),
        _ => throw new InvalidOperationException($"Unknown mouse button '{button}'.")
    };

    private static MouseInput GetButtonUp(string button) => button switch
    {
        "left" => MouseInput.LeftUp(),
        "right" => MouseInput.RightUp(),
        "middle" => MouseInput.MiddleUp(),
        _ => throw new InvalidOperationException($"Unknown mouse button '{button}'.")
    };

    private static Key[] GetRequiredKeys(JsonElement action, string propertyName, int index)
    {
        if (!action.TryGetProperty(propertyName, out var property) || property.ValueKind != JsonValueKind.Array)
        {
            throw new InvalidOperationException(
                $"Action at index {index} is missing required array property '{propertyName}'.");
        }

        List<Key> keys = [];
        int keyIndex = 0;
        foreach (var keyValue in property.EnumerateArray())
        {
            if (keyValue.ValueKind != JsonValueKind.String)
            {
                throw new InvalidOperationException(
                    $"Action at index {index} has invalid key value at '{propertyName}[{keyIndex}]'. Expected a key name string.");
            }

            string? rawKey = keyValue.GetString();
            if (string.IsNullOrWhiteSpace(rawKey) || !Enum.TryParse(rawKey, true, out Key key))
            {
                throw new InvalidOperationException(
                    $"Action at index {index} has unknown key '{rawKey}' at '{propertyName}[{keyIndex}]'.");
            }

            keys.Add(key);
            keyIndex++;
        }

        if (keys.Count == 0)
        {
            throw new InvalidOperationException(
                $"Action at index {index} must specify at least one key in '{propertyName}'.");
        }

        return [.. keys];
    }
}

