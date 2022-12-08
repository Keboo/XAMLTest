using System.CommandLine;
using System.IO;
using XamlTest.Internal;
using XamlTest.Utility;

namespace XamlTest;

internal class Program
{
    private static Timer? HeartbeatTimer { get; set; }

    [STAThread]
    static int Main(string[] args)
    {
        Logger.Log("Starting log");

        Argument<int> clientPid = new("clientPid");
        Option<string> appPath = new("--application-path");
        Option<string> appType = new("--application-type");

        Option<string> remoteMethod = new("--remote-method-name");
        Option<string> remoteContainerType = new("--remote-method-container-type");
        Option<string> remoteAssembly = new("--remote-method-assembly");

        Option<bool> debug = new("--debug");
        Option<FileInfo> logFile = new("--log-file");
        RootCommand command = new()
        {
            clientPid,
            appPath,
            appType,

            remoteMethod,
            remoteContainerType,
            remoteAssembly,

            debug,
            logFile
        };

        var parseResult = command.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            foreach(var error in parseResult.Errors)
            {
                Logger.Log(error.Message);
            }
            return -1;
        }

        int pidValue = parseResult.GetValueForArgument(clientPid);
        string? appPathValue = parseResult.GetValueForOption(appPath);
        string? appTypeValue = parseResult.GetValueForOption(appType);
        bool waitForDebugger = parseResult.GetValueForOption(debug);
        FileInfo? logFileInfo = parseResult.GetValueForOption(logFile);

        string? remoteMethodName = parseResult.GetValueForOption(remoteMethod);
        string? remoteContainerTypeValue = parseResult.GetValueForOption(remoteContainerType);
        string? remoteAssemblyValue = parseResult.GetValueForOption(remoteAssembly);

        try
        {
            if (logFileInfo is not null)
            {
                Logger.AddLogOutput(logFileInfo.Open(FileMode.Create, FileAccess.Write, FileShare.Read));
            }
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            
            Application application;
            if (!string.IsNullOrWhiteSpace(appPathValue) &&
                Path.GetFullPath(appPathValue) is { } fullPath &&
                File.Exists(fullPath))
            {
                application = CreateFromAssembly(fullPath, appTypeValue, remoteMethodName, remoteContainerTypeValue, remoteAssemblyValue);
            }
            else
            {
                application = new Application
                {
                    ShutdownMode = ShutdownMode.OnLastWindowClose
                };
            }

            IDisposable? service = null;

            application.Startup += ApplicationStartup;
            application.Exit += ApplicationExit;
            application.DispatcherUnhandledException += ApplicationUnhandledException;

            return application.Run();


            void ApplicationStartup(object sender, StartupEventArgs e)
            {
                Logger.Log("Starting XAMLTest server");
                service = Server.Start(application);
                Logger.Log("Started XAMLTest server");

                HeartbeatTimer = new(HeartbeatCheck, pidValue, TimeSpan.Zero, TimeSpan.FromSeconds(1));
                if (waitForDebugger)
                {
                    for (; !Debugger.IsAttached;)
                    {
                        Thread.Sleep(100);
                    }
                }
            }

            void ApplicationExit(object sender, ExitEventArgs e)
            {
                Logger.CloseLogger();
                service?.Dispose();
            }

            static void ApplicationUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
                => Logger.Log(e.ToString() ?? "Unhandled exception");

            static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
                => Logger.Log(e.ToString() ?? "Domain unhandled exception");


            void HeartbeatCheck(object? state)
            {
                var pid = (int)state!;
                bool shutdown = false;
                try
                {
                    using Process p = Process.GetProcessById(pid);
                    shutdown = p.HasExited;
                    if (shutdown)
                    {
                        Logger.Log($"Host process {pid} {(p is null ? "not found" : "has exited")}");
                    }
                }
                catch (Exception e)
                {
                    shutdown = true;
                    Logger.Log($"Error retrieving processes '{pid}' {e}");
                }

                if (shutdown)
                {
                    HeartbeatTimer?.Change(0, Timeout.Infinite);
                    application.Dispatcher.Invoke(() => application.Shutdown());
                }
            }
        }
        catch (Exception e)
        {
            Logger.Log(e.ToString());
            Logger.CloseLogger();
            return -2;
        }
    }

    private static Application CreateFromAssembly(
        string assemblyPath,
        string? applicationType,
        string? remoteMethodName,
        string? remoteContainerType,
        string? remoteAssembly)
    {
        AppDomain.CurrentDomain.IncludeAssembliesIn(Path.GetDirectoryName(assemblyPath)!);

        var targetAssembly = Assembly.LoadFile(assemblyPath);
        Application application;
        if (remoteMethodName != null &&
            remoteContainerType != null &&
            remoteAssembly != null)
        {
            Logger.Log($"Using factory method {remoteMethodName}() in {remoteContainerType} of {remoteAssembly}");
            var factoryAssembly = Assembly.LoadFrom(remoteAssembly);
            Type factoryType = factoryAssembly.GetType(remoteContainerType, throwOnError: true)!;
            var methodFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy;
            MethodInfo factoryMethod = factoryType.GetMethod(remoteMethodName, methodFlags) 
                ?? throw new XamlTestException($"Did not find factory method {remoteMethodName} in {remoteContainerType}");
            application = (Application)(factoryMethod.Invoke(null, Array.Empty<object>())
                ?? throw new XamlTestException("Factory method did return an application instance"));
        }
        else
        {
            Type appType;
            if (!string.IsNullOrWhiteSpace(applicationType))
            {
                appType = Type.GetType(applicationType, throwOnError: true)
                    ?? throw new XamlTestException($"Could not find Application type {applicationType}");
            }
            else
            {
                var applications = targetAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Application))).ToList();
                appType = applications.Count switch
                {
                    0 => throw new XamlTestException($"Could not find any Application types"),
                    1 => applications[0],
                    _ => throw new XamlTestException($"Found multiple Application types {string.Join(", ", applications.Select(x => x.FullName))}"),
                };
            }

            var ctorInfo = appType.GetConstructors().Single();
            var ctorParameters = ctorInfo.GetParameters();

            object?[] parameters = Array.Empty<object?>();
            if (ctorParameters.Length > 0)
            {
                parameters = new object?[ctorParameters.Length];
                for (int i = 0; i < ctorParameters.Length; i++)
                {
                    if (ctorParameters[i].HasDefaultValue)
                    {
                        parameters[i] = ctorParameters[i].DefaultValue;
                    }
                }
            }

            Logger.Log($"Creating application {appType.FullName}({string.Join(", ", parameters.Select(x => x?.ToString() ?? "null"))})");
            application = (Application)ctorInfo.Invoke(parameters);
        }
        
        BindingFlags flags = BindingFlags.Instance | BindingFlags.InvokeMethod | BindingFlags.FlattenHierarchy | BindingFlags.Public;
        if (application.GetType().GetMethod("InitializeComponent", flags) is { } initMethod)
        {
            Logger.Log("Invoking InitializeComponent");
            initMethod.Invoke(application, Array.Empty<object>());
        }
        else
        {
            Logger.Log("Did not find InitializeComponent method");
        }

        return application;
    }
}
