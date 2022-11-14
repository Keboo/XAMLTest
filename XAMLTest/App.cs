using GrpcDotNetNamedPipes;
using XamlTest.Host;

namespace XamlTest;

public static class App
{
    public static Task<IApp> StartRemote<TApp>(Action<string>? logMessage = null)
        where TApp : Application
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
        if (!string.IsNullOrWhiteSpace(options.ApplicationType))
        {
            startInfo.ArgumentList.Add("--application-type");
            startInfo.ArgumentList.Add(options.ApplicationType);
        }
        if (options.RemoteFactoryMethod is { } remoteFactoryMethod)
        {
            startInfo.ArgumentList.Add("--remote-method-name");
            startInfo.ArgumentList.Add(remoteFactoryMethod.MethodName);
            startInfo.ArgumentList.Add("--remote-method-container-type");
            startInfo.ArgumentList.Add(remoteFactoryMethod.MethodContainerType);
            startInfo.ArgumentList.Add("--remote-method-assembly");
            startInfo.ArgumentList.Add(remoteFactoryMethod.Assembly);
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
            string args = string.Join(' ', startInfo.ArgumentList.Select((x, i) => i % 2 == 1 ? x : $"\"{x}\""));
            logMessage($"Starting XAML Test: {startInfo.FileName} {args}");
        }

        if (Process.Start(startInfo) is Process process)
        {
            NamedPipeChannel channel = new(".", Server.PipePrefix + process.Id, new NamedPipeChannelOptions
            {
                ConnectionTimeout = (int)options.ConnectionTimeout.TotalMilliseconds
            });
            Protocol.ProtocolClient client = new(channel);
            if (useDebugger)
            {
                await VisualStudioAttacher.AttachVisualStudioToProcess(process);
            }

            var app = new Internal.App(process, client, options);

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
