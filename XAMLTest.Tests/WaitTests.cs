namespace XamlTest.Tests;

[TestClass]
public class WaitTests
{
    private Task<bool> Timeout()
    {
        return Task.FromResult(false);
    }

    private Task<bool> Success()
    {
        return Task.FromResult(true);
    }

    [TestMethod]
    public async Task ShouldTimeout()
    {
        await Assert.ThrowsAsync<TimeoutException>(async () => await Wait.For(Timeout));
    }

    [TestMethod]
    public async Task ShouldNotTimeout()
    {
        await Wait.For(Success);
    }

    [TestMethod]
    public async Task ShouldTimeoutWithMessage()
    {
        var timeoutMessage = "We're expecting a timeout";
        var ex = await Assert.ThrowsAsync<TimeoutException>(async () => await Wait.For(Timeout, message: timeoutMessage));
        Assert.StartsWith(timeoutMessage, ex.Message, $"Expected exception message to start with: '{timeoutMessage}'");
    }
}
