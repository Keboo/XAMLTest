using SimulatedApp = XamlTest.Tests.Simulators.App;

namespace XamlTest.Tests;

[TestClass]
[DoNotParallelize]
public class ResourceLockTests
{
    [TestMethod]
    public async Task AcquireResourceLocksAsync_WithNone_ReturnsNoOpLock()
    {
        await using var app = new SimulatedApp();
        await using var _ = await app.AcquireResourceLocksAsync(ResourceLocks.None);
    }

    [TestMethod]
    public async Task AcquireResourceLocksAsync_WithHeldLock_ThrowsTimeoutException()
    {
        await using var app = new SimulatedApp();
        await using var _ = await app.AcquireResourceLocksAsync(ResourceLocks.Keyboard);

        await Assert.ThrowsExactlyAsync<TimeoutException>(async () =>
        {
            await using var lock2 = await app.AcquireResourceLocksAsync(ResourceLocks.Keyboard, TimeSpan.FromMilliseconds(100));
        });
    }

    [TestMethod]
    public async Task AcquireResourceLocksAsync_CanAcquireComposedFlags()
    {
        await using var app = new SimulatedApp();
        await using var _ = await app.AcquireResourceLocksAsync(ResourceLocks.Input | ResourceLocks.Focus, TimeSpan.FromSeconds(1));
    }

    [TestMethod]
    public async Task AcquireResourceLocksAsync_UsesDefaultTimeoutWhenTimeoutNotSet()
    {
        await using var app = new SimulatedApp();
        await using var _ = await app.AcquireResourceLocksAsync(ResourceLocks.Mouse, TimeSpan.FromSeconds(1));
        using CancellationTokenSource cts = new(TimeSpan.FromMilliseconds(100));

        await Assert.ThrowsExactlyAsync<OperationCanceledException>(async () =>
        {
            await using var lock2 = await app.AcquireResourceLocksAsync(ResourceLocks.Mouse, cancellationToken: cts.Token);
        });
    }
}
