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
        AppOptions options = new()
        {
            LogMessage = msg => context.WriteLine(msg),
            AllowVisualStudioDebuggerAttach = true
        };
        App = await XamlTest.App.StartRemote(options);

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        Window = await App.CreateWindowWithContent(@"<Border />");
    }

    [ClassCleanup]
    public static async Task TestCleanup()
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
        Assert.AreEqual(initialCoordinates.Width, Math.Round(newCoordinates.Left - initialCoordinates.Left));
        Assert.AreEqual(initialCoordinates.Width, Math.Round(newCoordinates.Top - initialCoordinates.Top));
    }

    [TestMethod]
    public async Task OnGetCoordinate_ReturnsFractionalCoordinatesOfElement()
    {
        IVisualElement<Border> element = await Window.SetXamlContent<Border>(@"<Border x:Name=""MyBorder"" 
Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left""/>");

        Rect initialCoordinates = await element.GetCoordinates();
        await element.SetWidth(30.7);
        await element.SetHeight(40.3);
        await element.SetMargin(new Thickness(0.1));

        Rect newCoordinates = await element.GetCoordinates();
        Assert.AreEqual(30.7, Math.Round(newCoordinates.Width, 5));
        Assert.AreEqual(40.3, Math.Round(newCoordinates.Height, 5));
        Assert.AreEqual(0.1, Math.Round(newCoordinates.Left - initialCoordinates.Left, 5));
        Assert.AreEqual(0.1, Math.Round(newCoordinates.Top - initialCoordinates.Top, 5));
    }

    [TestMethod]
    public async Task OnGetCoordinate_ReturnsRotatedElementLocation()
    {
        IVisualElement<Border> element = await Window.SetXamlContent<Border>(@"
<Border x:Name=""MyBorder"" Width=""30"" Height=""40"" VerticalAlignment=""Top"" HorizontalAlignment=""Left"">
    <Border.LayoutTransform>
        <RotateTransform Angle=""90"" />
    </Border.LayoutTransform>
</Border>
");

        Rect coordinates = await element.GetCoordinates();
        Assert.AreEqual(40, Math.Round(coordinates.Width, 5));
        Assert.AreEqual(30, Math.Round(coordinates.Height, 5));
    }
}
