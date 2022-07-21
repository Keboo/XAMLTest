using System.Diagnostics;
using XamlTest.Internal;

namespace XamlTest;

internal static class Server
{
    internal const string PipePrefix = nameof(DependencyObjectTracker) + "ComminicationPipe";

    //TODO: This could probably be moved into the Service class
    internal static Service Start(Application? app = null)
    {
        var process = Process.GetCurrentProcess();
        Service service = new(process.Id.ToString(), app ?? Application.Current);
        return service;
    }
}
