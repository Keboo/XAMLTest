using System;
using System.CommandLine;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Windows;
using XamlTest.Utility;

namespace XamlTest;

internal class Program
{
    private static Timer? HeartbeatTimer { get; set; }

    [STAThread]
    static int Main(string[] args)
    {
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

        Application application;
        if (!string.IsNullOrWhiteSpace(appPathValue) &&
            Path.GetFullPath(appPathValue) is { } fullPath &&
            File.Exists(fullPath))
        {
            application = CreateFromAssembly(fullPath);
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
    }

    private static Application CreateFromAssembly(string assemblyPath)
    {
        AppDomain.CurrentDomain.IncludeAssembliesIn(Path.GetDirectoryName(assemblyPath)!);

        var targetAssembly = Assembly.LoadFile(assemblyPath);

        var appType = targetAssembly.GetTypes().Where(x => x.IsSubclassOf(typeof(Application))).Single();
        var application = (Application)appType.GetConstructors().Single().Invoke(new object[0]);

        if (appType.GetMethod("InitializeComponent") is { } initMethod)
        {
            initMethod.Invoke(application, Array.Empty<object>());
        }

        return application;
    }
}
