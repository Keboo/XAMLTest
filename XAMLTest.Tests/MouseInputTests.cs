namespace XamlTest.Tests;

[TestClass]
public class MouseInputTests
{
    [TestMethod]
    public void CanRetrieveMouseDoubleClickTime()
    {
        Assert.IsTrue(MouseInput.GetDoubleClickTime > TimeSpan.Zero);
    }

    [TestMethod]
    public void DoubleClickTimeIsAccessibleWithoutWinForms()
    {
        // Verify that GetDoubleClickTime works without needing WinForms reference
        TimeSpan doubleClickTime = MouseInput.GetDoubleClickTime;
        Assert.IsTrue(doubleClickTime.TotalMilliseconds > 0);
        
        // Typical double click time should be between 100ms and 1000ms
        Assert.IsTrue(doubleClickTime.TotalMilliseconds >= 100);
        Assert.IsTrue(doubleClickTime.TotalMilliseconds <= 1000);
    }

}
