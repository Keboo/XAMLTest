using System.Collections.Concurrent;

namespace XamlTest.Tests;

[TestClass]
public class AppDisposalTests
{
    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task AlreadyExitedProcess_DisposesSuccessfully(bool asynchronous)
    {
        IApp app = await StartApp();
        using Process process = await GetHostProcess(app);
        try
        {
            process.Kill();
            await process.WaitForExitAsync();

            await Dispose(app, asynchronous);

            Assert.IsTrue(process.HasExited);
        }
        finally
        {
            await EnsureProcessExited(process);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task TenConcurrentApps_CanStartAndDisposeRepeatedly(bool asynchronous)
    {
        for (int batch = 0; batch < 3; batch++)
        {
            await Task.WhenAll(Enumerable.Range(0, 10).Select(_ => Task.Run(async () =>
            {
                IApp app = await StartApp();
                using Process process = await GetHostProcess(app);
                try
                {
                    await Dispose(app, asynchronous);

                    Assert.IsTrue(process.HasExited);
                }
                finally
                {
                    await EnsureProcessExited(process);
                }
            })));
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SlowExit_DisposalWaitsForProcess(bool asynchronous)
    {
        ConcurrentQueue<string> messages = new();
        IApp app = await StartApp(messages.Enqueue);
        using Process process = await GetHostProcess(app);
        try
        {
            static void DelayExit(Application application)
            {
                application.Exit += (_, _) => Thread.Sleep(TimeSpan.FromSeconds(1.5));
            }
            await app.RemoteExecute<Application>(DelayExit);

            Stopwatch stopwatch = Stopwatch.StartNew();
            await Dispose(app, asynchronous);

            Assert.IsTrue(process.HasExited);
            Assert.IsTrue(stopwatch.Elapsed >= TimeSpan.FromSeconds(1.5));
            Assert.IsFalse(messages.Any(x => x.Contains("Invoking kill")));
        }
        finally
        {
            await EnsureProcessExited(process);
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task UnresponsiveExit_IsKilledAndReaped(bool asynchronous)
    {
        ConcurrentQueue<string> messages = new();
        IApp app = await StartApp(messages.Enqueue);
        using Process process = await GetHostProcess(app);
        try
        {
            static void BlockExit(Application application)
            {
                application.Exit += (_, _) => Thread.Sleep(Timeout.Infinite);
            }
            await app.RemoteExecute<Application>(BlockExit);

            Task disposal = Dispose(app, asynchronous);
            if (asynchronous)
            {
                Assert.IsFalse(disposal.IsCompleted, "Async disposal must yield while waiting for the host.");
            }
            await disposal;

            Assert.IsTrue(process.HasExited);
            Assert.IsTrue(messages.Any(x => x.Contains("Invoking kill")));
        }
        finally
        {
            await EnsureProcessExited(process);
        }
    }

    private static async Task<IApp> StartApp(Action<string>? logMessage = null)
    {
        IApp app = await App.StartRemote(new AppOptions
        {
            AllowVisualStudioDebuggerAttach = false,
            LogMessage = logMessage
        });
        try
        {
            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
            return app;
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }
    }

    private static async Task<Process> GetHostProcess(IApp app)
    {
        static int GetProcessId(Application _) => Environment.ProcessId;
        try
        {
            int processId = await app.RemoteExecute<Application, int>(GetProcessId);
            return Process.GetProcessById(processId);
        }
        catch
        {
            await app.DisposeAsync();
            throw;
        }
    }

    private static async Task EnsureProcessExited(Process process)
    {
        if (!process.HasExited)
        {
            process.Kill();
            await process.WaitForExitAsync().WaitAsync(TimeSpan.FromSeconds(10));
        }
    }

    private static async Task Dispose(IApp app, bool asynchronous)
    {
        if (asynchronous)
        {
            await app.DisposeAsync();
        }
        else
        {
            app.Dispose();
        }
    }
}
