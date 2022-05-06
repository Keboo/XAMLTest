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
        Action<string>? logMessage = null)
        => StartRemoteApp(remoteApp, xamlTestPath, logMessage, false).Result;

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
        Action<string>? logMessage = null)
        => await StartRemoteApp(remoteApp, xamlTestPath, logMessage, true);

    private static async Task<IApp> StartRemoteApp(
        string? remoteApp,
        string? xamlTestPath,
        Action<string>? logMessage,
        bool allowDebuggerAttach)
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

        if (Process.Start(startInfo) is Process process)
        {
            NamedPipeChannel channel = new(".", Server.PipePrefix + process.Id, new NamedPipeChannelOptions
            {
                ConnectionTimeout = 1000
            });
            Protocol.ProtocolClient client = new(channel);
            if (useDebugger)
            {
                await VisualStudioAttacher.AttachVisualStudioToProcess(process);
            }

            return new ManagedApp(process, client, logMessage);
        }
        throw new XAMLTestException("Failed to start remote app");
    }
}
