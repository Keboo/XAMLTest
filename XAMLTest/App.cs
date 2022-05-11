using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Threading.Tasks;
using GrpcDotNetNamedPipes;
using XamlTest.Host;
using XamlTest.Internal;

namespace XamlTest;

public static class App
{
    public static readonly TimeSpan DefaultConnectionTimeout = TimeSpan.FromSeconds(1);

    public static IApp StartRemote<TApp>(
        string? xamlTestPath = null,
        Action<string>? logMessage = null)
    {
        string location = typeof(TApp).Assembly.Location;
        return StartRemote(location, xamlTestPath, logMessage);
    }

    public static IApp StartRemote(
        string? remoteApp = null,
        string ? xamlTestPath = null,
        Action<string>? logMessage = null,
        TimeSpan? connectionTimeout = null)
        => StartRemoteApp(remoteApp, xamlTestPath, logMessage, false, connectionTimeout).Result;

    public static async Task<IApp> StartWithDebugger<TApp>(
        string? xamlTestPath = null,
        Action<string>? logMessage = null)
    {
        string location = typeof(TApp).Assembly.Location;
        return await StartWithDebugger(location, xamlTestPath, logMessage);
    }

    public static async Task<IApp> StartWithDebugger(
        string? remoteApp = null,
        string? xamlTestPath = null,
        Action<string>? logMessage = null,
        TimeSpan? connectionTimeout = null)
        => await StartRemoteApp(remoteApp, xamlTestPath, logMessage, true, connectionTimeout);

    private static async Task<IApp> StartRemoteApp(
        string? remoteApp,
        string? xamlTestPath,
        Action<string>? logMessage,
        bool allowDebuggerAttach,
        TimeSpan? connectionTimeout)
    {
        xamlTestPath ??= Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe");
        xamlTestPath = Path.GetFullPath(xamlTestPath);
        if (!File.Exists(xamlTestPath))
        {
            throw new XAMLTestException($"Could not find test app '{xamlTestPath}'");
        }

        ProcessStartInfo startInfo = new(xamlTestPath)
        {
            WorkingDirectory = Path.GetDirectoryName(xamlTestPath) + Path.DirectorySeparatorChar,
            UseShellExecute = true
        };
        startInfo.ArgumentList.Add($"{Process.GetCurrentProcess().Id}");
        if (!string.IsNullOrWhiteSpace(remoteApp))
        {
            startInfo.ArgumentList.Add("--application-path");
            startInfo.ArgumentList.Add(remoteApp);
        }
        bool useDebugger = allowDebuggerAttach && Debugger.IsAttached;
        if (useDebugger)
        {
            startInfo.ArgumentList.Add($"--debug");
        }

        if (logMessage is not null)
        {
            logMessage($"Starting XAML Test: {startInfo.FileName} {string.Join(' ', startInfo.ArgumentList)}");
        }

        if (Process.Start(startInfo) is Process process)
        {
            NamedPipeChannel channel = new(".", Server.PipePrefix + process.Id, new NamedPipeChannelOptions
            {
                ConnectionTimeout = (int)(connectionTimeout ?? DefaultConnectionTimeout).TotalMilliseconds,
                CurrentUserOnly = true
            });
            Protocol.ProtocolClient client = new(channel);
            if (useDebugger)
            {
                await VisualStudioAttacher.AttachVisualStudioToProcess(process);
            }

            var app = new ManagedApp(process, client, logMessage);

            await app.GetVersion(waitForReady:true);

            return app;
        }
        throw new XAMLTestException("Failed to start remote app");
    }
}
