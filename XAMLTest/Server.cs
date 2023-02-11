using XamlTest.Internal;

namespace XamlTest;

public static class Server
{
    internal const string PipePrefix = nameof(XamlTest) + ".CommunicationPipe.";

    internal static IDisposable Start(Application? app = null)
    {
        var process = Process.GetCurrentProcess();
        Service service = new(process.Id.ToString(), app ?? Application.Current);
        return service;
    }
}
