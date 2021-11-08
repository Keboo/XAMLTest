using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace XamlTest.Tests
{
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
            await Assert.ThrowsExceptionAsync<TimeoutException>(async () => await Wait.For(Timeout));
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
            var ex = await Assert.ThrowsExceptionAsync<TimeoutException>(async () => await Wait.For(Timeout, message: timeoutMessage));
            Assert.IsTrue(ex.Message.StartsWith(timeoutMessage), $"Expected exception message to start with: '{timeoutMessage}'");
           
        }
    }
}
