using System.Diagnostics;
using System.Windows;
using XAMLTest.Internal;

namespace XAMLTest
{
    public static class Server
    {
        internal const string PipePrefix = nameof(DependencyObjectTracker) + "ComminicationPipe";

        internal static IService Start(Application? app = null)
        {
            var process = Process.GetCurrentProcess();
            var service = new Service(process.Id.ToString(), app ?? Application.Current);
            return service;
        }
    }
}
