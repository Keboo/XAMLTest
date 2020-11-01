using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using XamlTest.Utility;

namespace XamlTest
{
    internal class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Application application;
            if (args?.Length == 1 &&
                Path.GetFullPath(args[0]) is { } fullPath &&
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
            application.Run();

            void ApplicationStartup(object sender, StartupEventArgs e)
                => service = Server.Start(application);

            void ApplicationExit(object sender, ExitEventArgs e)
                => service?.Dispose();
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
