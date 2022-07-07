using GrpcDotNetNamedPipes;
using System.Diagnostics;
using XamlTest.Host;
using XamlTest.Internal;

namespace XamlTest;

public static class App
{
    public static Task<IApp> StartRemote<TApp>(Action<string>? logMessage = null)
    {
        AppOptions options = new()
        {
            LogMessage = logMessage
        };
        options.WithRemoteApp<TApp>();
        return StartRemote(options);
    }

    public static Task<IApp> StartRemote(Action<string>? logMessage = null)
    {
        AppOptions options = new()
        {
            LogMessage = logMessage
        };
        return StartRemote(options);
    }

    public static async Task<IApp> StartRemote(AppOptions options)
    {
        if (!File.Exists(options.XamlTestPath))
        {
            throw new XAMLTestException($"Could not find test app '{options.XamlTestPath}'");
        }

        ProcessStartInfo startInfo = new(options.XamlTestPath)
        {
            WorkingDirectory = Path.GetDirectoryName(options.XamlTestPath) + Path.DirectorySeparatorChar,
            UseShellExecute = true
        };
#if NET6_0_OR_GREATER
        startInfo.ArgumentList.Add($"{Environment.ProcessId}");
#else
        startInfo.ArgumentList.Add($"{Process.GetCurrentProcess().Id}");
#endif
        if (!string.IsNullOrWhiteSpace(options.RemoteAppPath))
        {
            startInfo.ArgumentList.Add("--application-path");
            startInfo.ArgumentList.Add(options.RemoteAppPath);
        }
        bool useDebugger = options.AllowVisualStudioDebuggerAttach && Debugger.IsAttached;
        if (useDebugger)
        {
            startInfo.ArgumentList.Add($"--debug");
        }

        var logMessage = options.LogMessage;

        if (logMessage is not null)
        {
            logMessage($"Starting XAML Test: {startInfo.FileName} {string.Join(' ', startInfo.ArgumentList)}");
        }

        if (Process.Start(startInfo) is Process process)
        {
            if (useDebugger)
            {
                await VisualStudioAttacher.AttachVisualStudioToProcess(process);
            }
            NamedPipeChannel channel = new(".", Server.PipePrefix + process.Id, new NamedPipeChannelOptions
            {
                ConnectionTimeout = (int)options.ConnectionTimeout.TotalMilliseconds,
                CurrentUserOnly = true
            });
            Protocol.ProtocolClient client = new(channel);

            var app = new ManagedApp(process, client, options.LogMessage);

            IVersion version = await Wait.For(() => app.GetVersion(true));
            if (logMessage is not null)
            {
                logMessage($"XAML Test v{version.XamlTestVersion}, App Version v{version.AppVersion}");
            }
            return app;
        }
        throw new XAMLTestException("Failed to start remote app");
    }
}
