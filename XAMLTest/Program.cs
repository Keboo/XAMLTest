using System.CommandLine;
using System.Diagnostics;
using System.Threading;
using XamlTest.Utility;

namespace XamlTest;

internal class Program
{

#if WIN_UI
    [System.Runtime.InteropServices.DllImport("Microsoft.ui.xaml.dll")]
    private static extern void XamlCheckProcessRequirements();
#endif


    private static Timer? HeartbeatTimer { get; set; }

    [STAThread]
    static int Main(string[] args)
    {
        Debugger.Launch();
        Argument<int> clientPid = new("clientPid");
        Option<string> appPath = new("--application-path");
        Option<bool> debug = new("--debug");
        RootCommand command = new()
        {
            clientPid,
            appPath,
            debug
        };

        var parseResult = command.Parse(args);
        if (parseResult.Errors.Count > 0)
        {
            return -1;
        }

        int pidValue = parseResult.GetValueForArgument(clientPid);
        string? appPathValue = parseResult.GetValueForOption(appPath);
        bool waitForDebugger = parseResult.GetValueForOption(debug);
        if (waitForDebugger)
        {
            Debugger.Break();
            for (; !Debugger.IsAttached;)
            {
                Thread.Sleep(100);
            }
        }
        Application application;
        if (!string.IsNullOrWhiteSpace(appPathValue) &&
            Path.GetFullPath(appPathValue) is { } fullPath &&
            File.Exists(fullPath))
        {
            application = CreateFromAssembly(fullPath);
        }
        else
        {
//            application = new Application
//            {
//#if WPF
//                ShutdownMode = ShutdownMode.OnLastWindowClose
//#endif
//            };
        }

#if WIN_UI
        XamlCheckProcessRequirements();

        WinRT.ComWrappersSupport.InitializeComWrappers();
        Application.Start((p) => {
            var context = new Microsoft.UI.Dispatching.DispatcherQueueSynchronizationContext(Microsoft.UI.Dispatching.DispatcherQueue.GetForCurrentThread());
            SynchronizationContext.SetSynchronizationContext(context);
            new InternalApp();
        });
        return 0;
#endif
#if WPF
        IDisposable? service = null;
        application.Startup += ApplicationStartup;
        application.Exit += ApplicationExit;

        return application.Run();

        void ApplicationStartup(object sender, StartupEventArgs e)
        {
            service = Server.Start(application);
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
            => service?.Dispose();

        void HeartbeatCheck(object? state)
        {
            var pid = (int)state!;
            bool shutdown = false;
            try
            {
                using Process p = Process.GetProcessById(pid);
                shutdown = p is null || p.HasExited;
            }
            catch
            {
                shutdown = true;
            }

            if (shutdown)
            {
                HeartbeatTimer?.Change(0, Timeout.Infinite);
                application.Dispatcher.Invoke(() => application.Shutdown());
            }
        }
#endif
    }

    private static Application CreateFromAssembly(string assemblyPath)
    {
        AppDomain.CurrentDomain.IncludeAssembliesIn(Path.GetDirectoryName(assemblyPath)!);

        var targetAssembly = Assembly.LoadFile(assemblyPath);

        var appType = targetAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Application))).Single();
        var application = (Application)appType.GetConstructors().Single().Invoke(Array.Empty<object>());

        if (appType.GetMethod("InitializeComponent") is { } initMethod)
        {
            initMethod.Invoke(application, Array.Empty<object>());
        }

        return application;
    }
}
