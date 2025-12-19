using ModelContextProtocol.Protocol;
using ModelContextProtocol.Server;
using System.ComponentModel;
using XamlTest;
using XAMLTest.Mcp;

[McpServerToolType]
internal class AppTools(AppServiceManager appServiceManager) : BaseTools
{
    [McpServerTool]
    [Description("""
        Starts a new WPF application using the provided application XAML, referenced assemblies, and creates a window with the specified XAML.
        This return the XAMLTest Id of the application.
        """)]
    public async Task<string> StartApp(
    [Description("""
        The full XAML for a WPF window, including its contents.
        """)] string windowXaml,
    [Description("""
        This is the raw application XAML to initialize the WPF app with.
        """)] string? appXaml = null,
    [Description("""
        The full path to any additional assemblies to load into the application domain.
        """)] string?[]? additionalAssemblies = null)
    {
        var app = await App.StartRemote(new AppOptions()
        {
            AllowVisualStudioDebuggerAttach = false,
            MinimizeOtherWindows = false
        });

        if (!string.IsNullOrWhiteSpace(appXaml))
        {
            string[] assemblies = [];
            if (additionalAssemblies is not null)
            {
                assemblies = additionalAssemblies.Where(x => x is not null).ToArray()!;
            }
            await app.Initialize(appXaml, assemblies);
        }
        else
        {
            await app.InitializeWithDefaults();
        }

        var _ = await app.CreateWindow(windowXaml);

        return appServiceManager.RegisterApp(app);
    }

    [McpServerTool]
    [Description("""
        Starts a new WPF application and creates a window with the specified XAML snippet as its content.
        This return the XAMLTest Id of the application.
        """)]
    public async Task<string> StartAppWithXamlSnippet(
        [Description(SharedStrings.XamlSnippetDescription)] string xamlSnippet)
    {
        var app = await App.StartRemote(new AppOptions()
        {
            AllowVisualStudioDebuggerAttach = false,
            MinimizeOtherWindows = false
        });

        await app.InitializeWithDefaults();

        var _ = await app.CreateWindowWithContent(xamlSnippet);

        return appServiceManager.RegisterApp(app);
    }

    [McpServerTool]
    [Description("""
        Shuts down the specified XAML Test application.
        """)]
    public async Task<CallToolResult> ShutdownApp(
        [Description(SharedStrings.AppIdDescription)] string appId)
    {
        return (await appServiceManager.ShutdownApp(appId))
            ? Success()
            : Failure($"No known app with id '{appId}' is running");
    }

    [McpServerTool]
    [Description("""
        Captures a screenshot of the specified XAML Test application and returns it as an embedded resource.
        """)]
    public async Task<CallToolResult> SaveScreenshot(
        [Description(SharedStrings.AppIdDescription)] string appId
        )
    {
        if (appServiceManager.TryGetApp(appId, out var app))
        {
            IImage screenshot = await app.GetScreenshot();
            if (await app.GetMainWindow() is { } window)
            {
                //TOD: expose Activate window

            }
            ; //TODO handle multiple windows
            using MemoryStream memoryStream = new();
            await screenshot.Save(memoryStream);

            string filename = $"screenshot_{appId}_{DateTime.Now:yyyyMMdd_HHmmss}.jpg";
            byte[] imageData = memoryStream.ToArray();

            EmbeddedResourceBlock resourceBlock = new()
            {
                Resource = new BlobResourceContents
                {
                    Uri = $"file:///{filename}",
                    MimeType = System.Net.Mime.MediaTypeNames.Image.Jpeg,
                    Blob = Convert.ToBase64String(imageData)
                }
            };

            return new()
            {
                IsError = false,
                Content = [resourceBlock]
            };
        }
        return Failure($"No known app with id '{appId}' is running");
    }
}

