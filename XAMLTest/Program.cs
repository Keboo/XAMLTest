using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using XamlTest.Utility;

namespace XamlTest
{
    internal class Program
    {
        private static System.Threading.Timer? HeartbeatTimer { get; set; }

        [STAThread]
        static int Main(string[] args)
        {
            if (args.Length < 1 || !int.TryParse(args[0], out int clientPid))
            {
                return -1;
            }

            Application application;
            if (args.Length == 2 &&
                Path.GetFullPath(args[1]) is { } fullPath &&
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

            IService? service = null;

            application.Startup += ApplicationStartup;
            application.Exit += ApplicationExit;

            return application.Run();

            void ApplicationStartup(object sender, StartupEventArgs e)
            {
                service = Server.Start(application);
                HeartbeatTimer = new(HeartbeatCheck, clientPid, TimeSpan.Zero, TimeSpan.FromSeconds(1));
            }

            void ApplicationExit(object sender, ExitEventArgs e)
                => service?.Dispose();

            void HeartbeatCheck(object? state)
            {
                var pid = (int)state!;
                using Process p = Process.GetProcessById(pid);
                if (p is null || p.HasExited)
                {
                    HeartbeatTimer?.Change(0, System.Threading.Timeout.Infinite);
                    application.Shutdown();
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
}
