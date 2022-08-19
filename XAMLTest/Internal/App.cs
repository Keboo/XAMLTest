using Grpc.Core;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using XamlTest.Host;

namespace XamlTest.Internal;

internal class App : IApp
{
    public App(Protocol.ProtocolClient client, AppOptions appOptions)
    {
        Client = client ?? throw new ArgumentNullException(nameof(client));
        AppOptions = appOptions ?? throw new ArgumentNullException(nameof(appOptions));
    }

    protected Protocol.ProtocolClient Client { get; }
    protected AppOptions AppOptions { get; }
    protected Action<string>? LogMessage => AppOptions?.LogMessage;
    protected AppContext Context { get; } = new();

    public IList<XmlNamespace> DefaultXmlNamespaces => Context.DefaultNamespaces;

    public virtual void Dispose()
    {
        ShutdownRequest request = new()
        {
            ExitCode = 0
        };
        LogMessage?.Invoke($"{nameof(IApp)}.{nameof(Dispose)}()");
        try
        {
            if (Client.Shutdown(request) is { } reply)
            {
                if (reply.ErrorMessages.Any())
                {
                    throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new XAMLTestException("Failed to get a reply");
        }
        finally
        {
            CleanupLogFiles().Wait();
        }
    }

    public virtual async ValueTask DisposeAsync()
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
                    throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new XAMLTestException("Failed to get a reply");
        }
        catch (OperationCanceledException)
        { }
        finally
        {
            await CleanupLogFiles();
        }
    }

    private async Task CleanupLogFiles()
    {
        if (AppOptions.LogMessage is { } logMessage &&
            AppOptions.RemoteProcessLogFile is { } logFile)
        {
            logFile.Refresh();
            if (logFile.Exists)
            {
                logMessage("-- Remote log start --");
                using StreamReader sr = new(logFile.OpenRead());
                logMessage((await sr.ReadToEndAsync()).Trim());
                logMessage("-- Remote log end --");

            }
        }
        try
        {
            AppOptions.RemoteProcessLogFile?.Delete();
        }
        catch { }
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
                    throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return;
            }
            throw new XAMLTestException("Failed to get a reply");
        }
        catch (RpcException e)
        {
            throw new XAMLTestException($"Error communicating with host process", e);
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
                throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages) + Environment.NewLine + windowXaml);
            }
            return new Window(Client, reply.WindowsId, Context, LogMessage);
        }
        throw new XAMLTestException("Failed to get a reply");
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
                throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            return new Window(Client, reply.WindowsId, Context, LogMessage);
        }
        throw new XAMLTestException("Failed to get a reply");
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
                throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            if (!string.IsNullOrWhiteSpace(reply.ValueType))
            {
                return new Resource(reply.Key, reply.ValueType, reply.Value, Context);
            }
            throw new XAMLTestException($"Resource with key '{reply.Key}' not found");
        }

        throw new XAMLTestException("Failed to receive a reply");
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
                    throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return new BitmapImage(reply.Data);
            }
            throw new XAMLTestException("Failed to receive a reply");
        }
        catch (RpcException e)
        {
            throw new XAMLTestException($"Error communicating with host process", e);
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
                throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
            }
            Context.Serializer.AddSerializer(new T(), insertIndex);
            return;
        }
        throw new XAMLTestException("Failed to receive a reply");
    }

    public Task<IReadOnlyList<ISerializer>> GetSerializers()
        => Task.FromResult<IReadOnlyList<ISerializer>>(Context.Serializer.Serializers.AsReadOnly());

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
                    throw new XAMLTestException(string.Join(Environment.NewLine, reply.ErrorMessages));
                }
                return new Version(reply.AppVersion, reply.XamlTestVersion);
            }
            throw new XAMLTestException("Failed to receive a reply");
        }
        catch (RpcException e)
        {
            throw new XAMLTestException($"Error communicating with host process", e);
        }
    }

    public void AddXamlNamespace(string? prefix, string uri) => throw new NotImplementedException();
}
