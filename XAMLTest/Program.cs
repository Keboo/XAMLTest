using System;
using System.Windows;

namespace XamlTest
{
    internal class Program
    {
        [STAThread]
        static void Main()
        {
            Application application = new CustomApplication
            {
                ShutdownMode = ShutdownMode.OnLastWindowClose
            };
            application.Run();
        }

        private class CustomApplication : Application
        {
            public IService? Service { get; set; }

            protected override void OnStartup(StartupEventArgs e)
            {
                base.OnStartup(e);
                Service = Server.Start(this);
            }

            protected override void OnExit(ExitEventArgs e)
            {
                Service?.Dispose();
                base.OnExit(e);
            }
        }
    }
}
