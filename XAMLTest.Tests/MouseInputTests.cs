namespace XamlTest.Tests;

[TestClass]
public class MouseInputTests
{
    [TestMethod]
    public void CanRetrieveMouseDoubleClickTime()
    {
        Assert.IsTrue(MouseInput.GetDoubleClickTime > TimeSpan.Zero);
    }

}
