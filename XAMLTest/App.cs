using GrpcDotNetNamedPipes;
using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
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
        startInfo.ArgumentList.Add($"{Process.GetCurrentProcess().Id}");
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
        if (options.RemoteProcessLogFile is { } logFile)
        {
            startInfo.ArgumentList.Add($"--log-file");
            startInfo.ArgumentList.Add(logFile.FullName);
        }

        var logMessage = options.LogMessage;

        if (logMessage is not null)
        {
            logMessage($"Starting XAML Test: {startInfo.FileName} {string.Join(' ', startInfo.ArgumentList)}");
        }

        if (Process.Start(startInfo) is Process process)
        {
            NamedPipeChannel channel = new(".", Server.PipePrefix + process.Id, new NamedPipeChannelOptions
            {
                ConnectionTimeout = (int)options.ConnectionTimeout.TotalMilliseconds,
                CurrentUserOnly = true
            });
            Protocol.ProtocolClient client = new(channel);
            if (useDebugger)
            {
                await VisualStudioAttacher.AttachVisualStudioToProcess(process);
            }

            var app = new ManagedApp(process, client, options);

            IVersion version;
            try
            {
                version = await Wait.For(() => app.GetVersion());
            }
            catch(TimeoutException)
            {
                if (logMessage is not null)
                {
                    process.Refresh();
                    if (process.HasExited)
                    {
                        logMessage($"Remote process not running");
                    }
                }
                await app.DisposeAsync();
                throw;
            }
            if (logMessage is not null)
            {
                logMessage($"XAML Test v{version.XamlTestVersion}, App Version v{version.AppVersion}");
            }
            return app;
        }
        throw new XAMLTestException("Failed to start remote app");
    }
}
