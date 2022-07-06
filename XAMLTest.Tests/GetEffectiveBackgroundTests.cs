namespace XamlTest.Tests;

[TestClass]
public class GetEffectiveBackgroundTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    private static IWindow? Window { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        Window = await App.CreateWindowWithContent(@"");
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
    public async Task OnGetEffectiveBackground_ReturnsFirstOpaqueColor()
    {
        IVisualElement<Grid> grid = await Window.SetXamlContent<Grid>(@"
<Grid>
    <Border x:Name=""MyBorder"" />
</Grid>");
        IVisualElement<Border> border = await grid.GetElement<Border>();
        await grid.SetBackgroundColor(Colors.Red);

        Color background = await border.GetEffectiveBackground();

        Assert.AreEqual(Colors.Red, background);
    }

    [TestMethod]
    public async Task OnGetEffectiveBackground_ReturnsMergingOfTransparentColors()
    {
        var backgroundParent = Colors.Blue;
        var backgroundChild = Color.FromArgb(0xDD, 0, 0, 0);
        await Window.SetXamlContent($@"
<Border Background=""{backgroundParent}"">
  <Border x:Name=""MyBorder"" Background=""{backgroundChild}"" />
</Border>");
#if WPF
        await Window.SetBackgroundColor(Colors.Red);
#endif
        IVisualElement element = await Window.GetElement("MyBorder");

        Color background = await element.GetEffectiveBackground();

        var expected = backgroundChild.FlattenOnto(backgroundParent);
        Assert.AreEqual(expected, background);
    }

    [TestMethod]
    public async Task OnGetEffectiveBackground_ReturnsOpaquePanelColor()
    {
        await Window.SetXamlContent(@"
<Grid Background=""Red"">
    <TextBlock />
</Grid>
");
#if WPF
        await Window.SetBackgroundColor(Colors.Blue);
#endif
        IVisualElement element = await Window.GetElement<TextBlock>();

        Color background = await element.GetEffectiveBackground();

        Assert.AreEqual(Colors.Red, background);
    }

    [TestMethod]
    public async Task OnGetEffectiveBackground_StopsProcessingAtDefinedParent()
    {
        await Window.SetXamlContent(@"
<StackPanel>
    <Grid Background=""#DDFF0000"">
        <TextBlock />
    </Grid>
</StackPanel>
");
        IVisualElement<StackPanel> stackPanel = await Window.GetElement<StackPanel>();
        IVisualElement child = await Window.GetElement<TextBlock>();
        IVisualElement parent = await Window.GetElement<Grid>();
        await stackPanel.SetBackgroundColor(Colors.Blue);

        Color background = await child.GetEffectiveBackground(parent);

        Assert.AreEqual(Color.FromArgb(0xDD, 0xFF, 0x00, 0x00), background);
    }

    [TestMethod]
    public async Task OnGetEffectiveBackground_AppliesOpacityFromParents()
    {
        await Window.SetXamlContent(@"
<Grid Background=""Lime"">
    <Grid Background=""Red"" Opacity=""0.5"" x:Name=""RedGrid"">
        <Grid Background=""Blue"" x:Name=""BlueGrid"">
            <TextBlock />
        </Grid>
    </Grid>
</Grid>
");

        IVisualElement child = await Window.GetElement<TextBlock>();
        IVisualElement parent = await Window.GetElement<Grid>("BlueGrid");

        Color background = await child.GetEffectiveBackground(parent);

        Assert.AreEqual(Color.FromArgb(127, 0x00, 0x00, 0xFF), background);
    }
}
