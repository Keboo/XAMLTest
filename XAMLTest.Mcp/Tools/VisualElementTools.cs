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

