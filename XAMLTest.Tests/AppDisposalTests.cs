using Grpc.Core;
using XamlTest.Host;

namespace XamlTest.Tests;

[TestClass]
public class AppDisposalTests
{
    [TestMethod]
    [DataRow(false, false)]
    [DataRow(true, false)]
    [DataRow(false, true)]
    [DataRow(true, true)]
    public async Task LocallyCancelledShutdown_DoesNotFailDisposal(bool asynchronous, bool rpcCancellation)
    {
        using Process process = await StartExitedProcess();
        List<string> messages = [];
        var client = new ShutdownClient(async options =>
        {
            try
            {
                await Task.Delay(Timeout.Infinite, options.CancellationToken);
            }
            catch (OperationCanceledException) when (options.CancellationToken.IsCancellationRequested)
            {
                if (!rpcCancellation)
                {
                    throw;
                }
            }
            throw new RpcException(new Status(StatusCode.Cancelled, ""));
        });
        var app = new Internal.App(process, client, new AppOptions { LogMessage = messages.Add });

        await Dispose(app, asynchronous);

        Assert.IsTrue(messages.Any(x => x.Contains("Shutdown request timed out")));
        Assert.IsTrue(process.HasExited);
    }

    [TestMethod]
    [DataRow(false, StatusCode.Cancelled)]
    [DataRow(true, StatusCode.Cancelled)]
    [DataRow(false, StatusCode.Internal)]
    [DataRow(true, StatusCode.Internal)]
    public async Task UnexpectedRpcError_IsNotSuppressed(bool asynchronous, StatusCode statusCode)
    {
        using Process process = await StartExitedProcess();
        var client = new ShutdownClient(_ => Task.FromException<ShutdownResult>(
            new RpcException(new Status(statusCode, "Unexpected shutdown error"))));
        var app = new Internal.App(process, client, new AppOptions());

        RpcException exception = await Assert.ThrowsAsync<RpcException>(() => Dispose(app, asynchronous));

        Assert.AreEqual(statusCode, exception.StatusCode);
        Assert.IsTrue(process.HasExited);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task AlreadyExitedProcess_DisposesSuccessfully(bool asynchronous)
    {
        using Process process = await StartExitedProcess();
        var client = new ShutdownClient(_ => Task.FromResult(new ShutdownResult()));
        var app = new Internal.App(process, client, new AppOptions());

        await Dispose(app, asynchronous);

        Assert.IsTrue(process.HasExited);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task SlowShutdownReply_DoesNotHitFormerOneSecondTimeout(bool asynchronous)
    {
        using Process process = await StartExitedProcess();
        var client = new ShutdownClient(async options =>
        {
            await Task.Delay(TimeSpan.FromSeconds(1.5), options.CancellationToken);
            return new ShutdownResult();
        });
        var app = new Internal.App(process, client, new AppOptions());

        await Dispose(app, asynchronous);
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task ShutdownErrorReply_IsNotSuppressed(bool asynchronous)
    {
        using Process process = await StartExitedProcess();
        var client = new ShutdownClient(_ =>
        {
            ShutdownResult reply = new();
            reply.ErrorMessages.Add("Shutdown failed");
            return Task.FromResult(reply);
        });
        var app = new Internal.App(process, client, new AppOptions());

        XamlTestException exception = await Assert.ThrowsAsync<XamlTestException>(() => Dispose(app, asynchronous));

        Assert.AreEqual("Shutdown failed", exception.Message);
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
                IApp app = await App.StartRemote(new AppOptions
                {
                    AllowVisualStudioDebuggerAttach = false
                });
                var host = (Internal.App)app;
                try
                {
                    await Dispose(host, asynchronous);
                    Assert.IsTrue(host.Process.HasExited);
                }
                finally
                {
                    if (!host.Process.HasExited)
                    {
                        host.Process.Kill();
                        await host.Process.WaitForExitAsync();
                    }
                    host.Process.Dispose();
                }
            })));
        }
    }

    [TestMethod]
    [DataRow(false)]
    [DataRow(true)]
    public async Task UnresponsiveProcess_IsKilledAndReaped(bool asynchronous)
    {
        using Process process = Process.Start(new ProcessStartInfo("powershell.exe",
            "-NoProfile -NonInteractive -Command Start-Sleep -Seconds 120")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        })!;
        try
        {
            var client = new ShutdownClient(_ => Task.FromResult(new ShutdownResult()));
            var app = new Internal.App(process, client, new AppOptions());

            Task disposal = Dispose(app, asynchronous);
            if (asynchronous)
            {
                Assert.IsFalse(disposal.IsCompleted, "Async disposal must yield while waiting for the host.");
            }
            await disposal;

            Assert.IsTrue(process.HasExited);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill();
                await process.WaitForExitAsync();
            }
        }
    }

    private static async Task<Process> StartExitedProcess()
    {
        Process process = Process.Start(new ProcessStartInfo("cmd.exe", "/c exit 0")
        {
            UseShellExecute = false,
            CreateNoWindow = true
        })!;
        await process.WaitForExitAsync();
        return process;
    }

    private static async Task Dispose(Internal.App app, bool asynchronous)
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

    private sealed class ShutdownClient(Func<CallOptions, Task<ShutdownResult>> shutdown) : Protocol.ProtocolClient
    {
        public override ShutdownResult Shutdown(ShutdownRequest request, CallOptions options)
            => shutdown(options).GetAwaiter().GetResult();

        public override AsyncUnaryCall<ShutdownResult> ShutdownAsync(ShutdownRequest request, CallOptions options)
            => new(shutdown(options), Task.FromResult(new Metadata()), () => Status.DefaultSuccess,
                () => new Metadata(), () => { });
    }
}
