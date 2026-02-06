namespace XamlTest.Tests;

[TestClass]
public class MouseInputTests
{
    [TestMethod]
    public void CanRetrieveMouseDoubleClickTime()
    {
        Assert.IsGreaterThan(TimeSpan.Zero, MouseInput.GetDoubleClickTime);
    }

    [TestMethod]
    public void DoubleClickTimeIsAccessibleWithoutWinForms()
    {
        // Verify that GetDoubleClickTime works without needing WinForms reference
        TimeSpan doubleClickTime = MouseInput.GetDoubleClickTime;
        Assert.IsGreaterThan(0, doubleClickTime.TotalMilliseconds);
        
        // Typical double click time should be between 100ms and 1000ms
        Assert.IsGreaterThanOrEqualTo(100, doubleClickTime.TotalMilliseconds);
        Assert.IsLessThanOrEqualTo(1000, doubleClickTime.TotalMilliseconds);
    }
}
