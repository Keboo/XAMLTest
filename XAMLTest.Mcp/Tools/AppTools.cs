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
        Captures a screenshot of the specified XAML Test application and returns it as inline BMP image content.
        """)]
    public async Task<CallToolResult> SaveScreenshot(string appId,
        [Description("Optional file path locations for where to same the image")]
        string? filePath = null)
    {
        if (!appServiceManager.TryGetApp(appId, out var app))
        {
            return Failure($"No known app with id '{appId}' is running");
        }

        IImage screenshot = await app.GetScreenshot();

        using MemoryStream bmpStream = new();
        await screenshot.Save(bmpStream);
        bmpStream.Position = 0;
        byte[] bmpBytes = bmpStream.ToArray();

        if (filePath is not null)
        {
            await File.WriteAllBytesAsync(filePath, bmpBytes);
        }

        return new CallToolResult
        {
            IsError = false,
            Content =
            [
                new TextContentBlock { Text = $"Screenshot for {appId}:" },
                ImageContentBlock.FromBytes(bmpBytes, "image/bmp")
            ]
        };
    }
}

