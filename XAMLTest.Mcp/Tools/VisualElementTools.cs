using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
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
}

