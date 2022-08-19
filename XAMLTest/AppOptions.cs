using System;
using System.IO;
using System.Reflection;

namespace XamlTest;

public class AppOptions
{
    private string? _xamlTestPath;

    public string? RemoteAppPath { get; set; }
    public string XamlTestPath
    {
        get
        {
            if (_xamlTestPath is { } path)
            {
                return path;
            }
            var xamlTestPath = Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe");
            return Path.GetFullPath(xamlTestPath);
        }

        set => _xamlTestPath = value;
    }
    public Action<string>? LogMessage { get; set; }
    public FileInfo? RemoteProcessLogFile 
    {
        get;
    }
    public bool AllowVisualStudioDebuggerAttach { get; set; } = true;
    public TimeSpan ConnectionTimeout { get; set; } = TimeSpan.FromSeconds(1);

    public AppOptions()
    {
        var directory = Path.GetDirectoryName(XamlTestPath);
        var file = Path.ChangeExtension(Path.GetRandomFileName(), ".xamltest.log");
        RemoteProcessLogFile = new FileInfo(Path.Combine(directory!, file));
    }

    public void WithRemoteApp<TApp>()
    {
        RemoteAppPath = typeof(TApp).Assembly.Location;
    }
}
