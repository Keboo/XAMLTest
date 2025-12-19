
using XamlTest;

namespace XAMLTest.Mcp;

public class AppServiceManager : IAsyncDisposable
{
    private Dictionary<string, IApp> RunningApps { get; } = [];

    public async ValueTask DisposeAsync()
    {
        List<IApp> apps;
        lock (RunningApps)
        {
            apps = [.. RunningApps.Values];
            RunningApps.Clear();
        }
        await Task.WhenAll(apps.Select(app => app.DisposeAsync().AsTask()));
    }

    public bool TryGetApp(string appId, [NotNullWhen(true)] out IApp? app)
    {
        lock (RunningApps)
        {
            return RunningApps.TryGetValue(appId, out app);
        }
    }

    public async Task<bool> ShutdownApp(string appId)
    {
        IApp? app;
        lock (RunningApps)
        {
            if (RunningApps.TryGetValue(appId, out app))
            {
                RunningApps.Remove(appId);
            }
        }
        if (app is not null)
        {
            await app.DisposeAsync();
            return true;
        }
        return false;
    }

    public string RegisterApp(IApp app)
    {
        string appId;
        lock (RunningApps)
        {
            appId = $"app{RunningApps.Count + 1}";
            RunningApps[appId] = app;
        }
        return appId;
    }
}