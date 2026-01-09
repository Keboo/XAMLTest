using Grpc.Core;
using XamlTest.Host;

namespace XamlTest.Internal;

internal sealed class App(
    Process process,
    Protocol.ProtocolClient client,
    AppOptions appOptions) : IApp
{
    public Process Process { get; } = process ?? throw new ArgumentNullException(nameof(process));

    private Protocol.ProtocolClient Client { get; } = client ?? throw new ArgumentNullException(nameof(client));
    private AppOptions AppOptions { get; } = appOptions ?? throw new ArgumentNullException(nameof(appOptions));

    void IApp.LogMessage(string message) => LogMessage?.Invoke(message);

    private Action<string>? LogMessage => line => AppOptions?.LogMessage?.Invoke($"{DateTime.Now} - {line}");
    private AppContext Context { get; } = new();

    public IList<XmlNamespace> DefaultXmlNamespaces => Context.DefaultNamespaces;

    public void Dispose()
    {
        ShutdownRequest request = new()
        {
            ExitCode = 0
        };
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(Dispose)}()");
        try
        {
            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(1));
            if (Client.Shutdown(request, cancellationToken: cts.Token) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new XamlTestException("Failed to get a reply");
        }
        catch (OperationCanceledException)
        { }
        catch (RpcException rpcException) when (rpcException.StatusCode == StatusCode.Unavailable)
        { }
        finally
        {
            KillProcess();
            CleanupLogFiles();
        }
    }

    public async ValueTask DisposeAsync()
    {
        ShutdownRequest request = new()
        {
            ExitCode = 0
        };
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(DisposeAsync)}()");
        try
        {
            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(1));
            if (await Client.ShutdownAsync(request, cancellationToken: cts.Token) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new XamlTestException("Failed to get a reply");
        }
        catch (OperationCanceledException)
        { }
        catch(RpcException rpcException) when (rpcException.StatusCode == StatusCode.Unavailable)
        { }
        finally
        {
            KillProcess();
            await CleanupLogFilesAsync();
        }
    }

    private async Task CleanupLogFilesAsync()
    {
        if (AppOptions.LogMessage is { } logMessage &&
            AppOptions.RemoteProcessLogFile is { } logFile)
        {
            if (File.Exists(logFile.FullName))
            {
                logMessage($"-- Remote log {logFile.FullName} --");
                using StreamReader sr = new(logFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                logMessage((await sr.ReadToEndAsync()).Trim());
                logMessage("-- Remote log end --");
            }
            else
            {
                logMessage("-- Remote log file not found --");
            }
        }
        try
        {
            AppOptions.RemoteProcessLogFile?.Delete();
        }
        catch { }
    }

    private void CleanupLogFiles()
    {
        if (AppOptions.LogMessage is { } logMessage &&
            AppOptions.RemoteProcessLogFile is { } logFile)
        {
            logFile.Refresh();
            if (logFile.Exists)
            {
                logMessage($"-- Remote log start {logFile.FullName} --");
                using StreamReader sr = new(logFile.Open(FileMode.Open, FileAccess.Read, FileShare.ReadWrite));
                logMessage(sr.ReadToEnd().Trim());
                logMessage("-- Remote log end --");

            }
        }
        try
        {
            AppOptions.RemoteProcessLogFile?.Delete();
        }
        catch { }
    }

    private void KillProcess()
    {
        LogMessage?.Invoke("Waiting for process exit");
        using CancellationTokenSource cts = new();
        cts.CancelAfter(TimeSpan.FromSeconds(10));
        Process? process = null;
        do
        {
            try
            {
                process = Process.GetProcessById(Process.Id);
            }
            catch (ArgumentException)
            {
                //Thrown when the process specified by the processId parameter is not running.
            }
        }
        while (process?.HasExited == false && !cts.IsCancellationRequested);

        LogMessage?.Invoke($"Process Exited? {process?.HasExited}");
        if (process?.HasExited == false)
        {
            LogMessage?.Invoke($"Invoking kill");
            process.Kill();
            process.WaitForExit(1_000);
        }
    }

    public async Task Initialize(string applicationResourceXaml, params string[] assemblies)
    {
        ApplicationConfiguration request = new()
        {
            ApplicationResourceXaml = applicationResourceXaml
        };
        request.AssembliesToLoad.AddRange(assemblies);
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(Initialize)}(...)");
        try
        {
            if (await Client.InitializeApplicationAsync(request) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new XamlTestException("Failed to get a reply");
        }
        catch (RpcException e)
        {
            throw new XamlTestException($"Error communicating with host process", e);
        }
    }

    public async Task<IWindow> CreateWindow(string windowXaml)
    {
        WindowConfiguration request = new()
        {
            Xaml = windowXaml,
            FitToScreen = true
        };
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(CreateWindow)}(...)");
        if (await Client.CreateWindowAsync(request) is { } reply)
        {
            if (LogMessage is { })
            {
                foreach (string logsMessage in reply.LogMessages)
                {
                    LogMessage(logsMessage);
                }
            }
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages) + Environment.NewLine + windowXaml);
            }
            return new Window(Client, reply.WindowsId, Context, LogMessage);
        }
        throw new XamlTestException("Failed to get a reply");
    }

    public async Task<IWindow> CreateWindow<TWindow>() where TWindow : System.Windows.Window
    {
        WindowConfiguration request = new()
        {
            WindowType = typeof(TWindow).AssemblyQualifiedName,
            FitToScreen = true
        };
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(CreateWindow)}(...)");
        if (await Client.CreateWindowAsync(request) is { } reply)
        {
            if (LogMessage is { })
            {
                foreach (string logsMessage in reply.LogMessages)
                {
                    LogMessage(logsMessage);
                }
            }
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return new Window(Client, reply.WindowsId, Context, LogMessage);
        }
        throw new XamlTestException("Failed to get a reply");
    }

    public async Task<IWindow?> GetMainWindow()
    {
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(GetMainWindow)}()");
        if (await Client.GetMainWindowAsync(new GetWindowsQuery()) is { } reply &&
            reply.WindowIds.Count == 1)
        {
            return new Window(Client, reply.WindowIds[0], Context, LogMessage);
        }
        return null;
    }

    public async Task<IResource> GetResource(string key)
    {
        ResourceQuery query = new()
        {
            Key = key
        };
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(GetResource)}()");
        if (await Client.GetResourceAsync(query) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            if (!string.IsNullOrWhiteSpace(reply.ValueType))
            {
                return new Resource(reply.Key, reply.ValueType, reply.Value, Context);
            }
            throw new XamlTestException($"Resource with key '{reply.Key}' not found");
        }

        throw new XamlTestException("Failed to receive a reply");
    }

    public async Task<IReadOnlyList<IWindow>> GetWindows()
    {
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(GetWindows)}()");
        if (await Client.GetWindowsAsync(new GetWindowsQuery()) is { } reply)
        {
            return reply.WindowIds.Select(x => new Window(Client, x, Context, LogMessage)).ToList();
        }
        return Array.Empty<IWindow>();
    }

    public async Task<IImage> GetScreenshot()
    {
        LogMessage?.Invoke($"{nameof(GetScreenshot)}()");
        ImageQuery imageQuery = new();
        try
        {
            if (await Client.GetScreenshotAsync(imageQuery) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return new BitmapImage(reply.Data);
            }
            throw new XamlTestException("Failed to receive a reply");
        }
        catch (RpcException e)
        {
            throw new XamlTestException($"Error communicating with host process", e);
        }
    }

    public async Task RegisterSerializer<T>(int insertIndex = 0)
        where T : ISerializer, new()
    {
        SerializerRequest request = new()
        {
            SerializerType = typeof(T).AssemblyQualifiedName,
            InsertIndex = insertIndex
        };
        if (await Client.RegisterSerializerAsync(request) is { } reply)
        {
            if (reply.ErrorMessages.Any())
            {
                throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            Context.Serializer.AddSerializer(new T(), insertIndex);
            return;
        }
        throw new XamlTestException("Failed to receive a reply");
    }

    public Task<IReadOnlyList<ISerializer>> GetSerializers()
        => Task.FromResult<IReadOnlyList<ISerializer>>(Context.Serializer.Serializers.AsReadOnly());

    public Task<TReturn?> RemoteExecute<TReturn>(Delegate @delegate, object?[] parameters)
        => Client.RemoteExecute<TReturn>(Context.Serializer, LogMessage, x => x.UseAppAsElement = true, @delegate, parameters);

    public async Task<IVersion> GetVersion()
    {
        LogMessage?.Invoke($"{nameof(GetVersion)}()");
        VersionRequest versionRequest = new();
        try
        {
            if (await Client.GetVersionAsync(versionRequest) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new XamlTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return new Version(reply.AppVersion, reply.XamlTestVersion);
            }
            throw new XamlTestException("Failed to receive a reply");
        }
        catch (RpcException e)
        {
            throw new XamlTestException($"Error communicating with host process", e);
        }
    }
}
