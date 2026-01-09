using System.Windows.Controls;
using XamlTest.Tests.TestControls;

namespace XamlTest.Tests;

[TestClass]
public class PositionTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    public static IVisualElement<MouseClickPositions>? UserControl { get; set; }

    [NotNull]
    public static IVisualElement<TextBlock>? PositionTextElement { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        var window = await App.CreateWindowWithUserControl<MouseClickPositions>(windowSize: new(400, 300));
        UserControl = await window.GetElement<MouseClickPositions>("/MouseClickPositions");
        PositionTextElement = await UserControl.GetElement<TextBlock>("ClickLocation");
    }

    [ClassCleanup(Microsoft.VisualStudio.TestTools.UnitTesting.InheritanceBehavior.BeforeEachDerivedClass)]
    public static async Task TestCleanup()
    {
        if (App is { } app)
        {
            await app.DisposeAsync();
            App = null;
        }
    }

    [TestMethod]
    public async Task CanClick_Center()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.Center);

        Assert.AreEqual(coordinates.Left + coordinates.Width / 2.0, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Top + coordinates.Height / 2.0, clickPosition.Y, 2.0);
    }

    [TestMethod]
    public async Task CanClick_TopLeft()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.TopLeft);

        Assert.AreEqual(coordinates.Left, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Top, clickPosition.Y, 2.0);
    }

    [TestMethod]
    public async Task CanClick_TopCenter()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.TopCenter);

        Assert.AreEqual(coordinates.Left + coordinates.Width / 2.0, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Top, clickPosition.Y, 2.0);
    }

    [TestMethod]
    public async Task CanClick_TopRight()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.TopRight);

        Assert.AreEqual(coordinates.Right, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Top, clickPosition.Y, 2.0);
    }

    [TestMethod]
    public async Task CanClick_RightCenter()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.RightCenter);

        Assert.AreEqual(coordinates.Right, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Top + coordinates.Height / 2.0, clickPosition.Y, 2.0);
    }

    [TestMethod]
    public async Task CanClick_BottomRight()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.BottomRight);

        Assert.AreEqual(coordinates.Right, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Bottom, clickPosition.Y, 2.0);
    }

    [TestMethod]
    public async Task CanClick_BottomCenter()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.BottomCenter);

        Assert.AreEqual(coordinates.Left + coordinates.Width / 2.0, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Bottom, clickPosition.Y, 2.0);
    }

    [TestMethod]
    public async Task CanClick_BottomLeft()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.BottomLeft);

        Assert.AreEqual(coordinates.Left, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Bottom, clickPosition.Y, 2.0);
    }

    [TestMethod]
    public async Task CanClick_LeftCenter()
    {
        Rect coordinates = await UserControl.GetCoordinates();
        Point clickPosition = await UserControl.LeftClick(Position.LeftCenter);

        Assert.AreEqual(coordinates.Left, clickPosition.X, 2.0);
        Assert.AreEqual(coordinates.Top + coordinates.Height / 2.0, clickPosition.Y, 2.0);
    }
}
