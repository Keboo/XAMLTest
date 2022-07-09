using GrpcDotNetNamedPipes;
using XamlTest.Host;

#if WIN_UI
using Microsoft.UI.Dispatching;
#endif
namespace XamlTest.Internal;

internal class Service : IDisposable
{
    private NamedPipeServer Server { get; }
    private bool IsDisposed { get; set; }

    public Service(string id, Application application)
    {
        if (application is null)
        {
            throw new ArgumentNullException(nameof(application));
        }

        Server = new NamedPipeServer(XamlTest.Server.PipePrefix + id);

#if WIN_UI
        Protocol.BindService(Server.ServiceBinder, new TestService(application, DispatcherQueue.GetForCurrentThread()));
#else
        Protocol.BindService(Server.ServiceBinder, new TestService(application));
#endif
        Server.Start();
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!IsDisposed)
        {
            if (disposing)
            {
                Server.Dispose();
            }

            IsDisposed = true;
        }
    }

    public void Dispose() => Dispose(true);
}
