using System.Windows.Controls;

namespace XamlTest.Tests;

[TestClass]
public class GetCoordinatesTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    private static IWindow? Window { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        Window = await App.CreateWindowWithContent(@"<Border />");
    }

    [ClassCleanup(ClassCleanupBehavior.EndOfClass)]
    public static async Task ClassCleanup()
    {
        if (App is { } app)
        {
            await app.DisposeAsync();
            App = null;
        }
    }

    [TestMethod]
    public async Task OnGetCoordinate_ReturnsScreenCoordinatesOfElement()
    {
        IVisualElement<Border> element = await Window.SetXamlContent<Border>(@"<Border x:Name=""MyBorder"" 
Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left""/>");
        
        Rect initialCoordinates = await element.GetCoordinates();
        await element.SetWidth(90);
        await element.SetHeight(80);
        await element.SetMargin(new Thickness(30));

        Rect newCoordinates = await element.GetCoordinates();
        Assert.AreEqual(3.0, Math.Round(newCoordinates.Width / initialCoordinates.Width));
        Assert.AreEqual(2.0, Math.Round(newCoordinates.Height / initialCoordinates.Height));
        Assert.AreEqual(initialCoordinates.Width, newCoordinates.Left - initialCoordinates.Left);
        Assert.AreEqual(initialCoordinates.Width, newCoordinates.Top - initialCoordinates.Top);
    }

    [TestMethod]
    public async Task OnGetCoordinate_ReturnsFractionalCoordinatesOfElement()
    {
        DpiScale scale = await Window.GetScale();
        IVisualElement<Border> element = await Window.SetXamlContent<Border>(@"<Border x:Name=""MyBorder"" 
Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left""/>");

        //38.375
        Rect initialCoordinates = await element.GetCoordinates();
        await element.SetWidth(await element.GetWidth() + 0.7);
        await element.SetHeight(await element.GetHeight() + 0.3);
        await element.SetMargin(new Thickness(0.1));

        Rect newCoordinates = await element.GetCoordinates();
        Assert.AreEqual(initialCoordinates.Width + (0.7 * scale.DpiScaleX), newCoordinates.Width, 0.00001);
        Assert.AreEqual(initialCoordinates.Height + (0.3 * scale.DpiScaleY), newCoordinates.Height, 0.00001);
        Assert.AreEqual(0.1 * scale.DpiScaleX, Math.Round(newCoordinates.Left - initialCoordinates.Left, 5), 0.00001);
        Assert.AreEqual(0.1 * scale.DpiScaleY, Math.Round(newCoordinates.Top - initialCoordinates.Top, 5), 0.00001);
    }

    [TestMethod]
    public async Task OnGetCoordinate_ReturnsRotatedElementLocation()
    {
        DpiScale scale = await Window.GetScale();

        IVisualElement<Border> element = await Window.SetXamlContent<Border>(@"
<Border x:Name=""MyBorder"" Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left"">
    <Border.LayoutTransform>
        <RotateTransform Angle=""90"" />
    </Border.LayoutTransform>
</Border>
");
        App.LogMessage("Before");
        Rect coordinates = await element.GetCoordinates();
        App.LogMessage("After");

        Assert.AreEqual(40 * scale.DpiScaleX, coordinates.Width, 0.00001);
        App.LogMessage("Assert1");
        Assert.AreEqual(30 * scale.DpiScaleY, coordinates.Height, 0.00001);
        App.LogMessage("Assert2");
    }
}