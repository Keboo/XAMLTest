using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;

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
        await Window.SetXamlContent(@"<Border x:Name=""MyBorder"" />");
        await Window.SetBackgroundColor(Colors.Red);

        IVisualElement element = await Window.GetElement("MyBorder");

        Color background = await element.GetEffectiveBackground();

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
        await Window.SetBackgroundColor(Colors.Red);

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
        await Window.SetBackgroundColor(Colors.Blue);
        
        IVisualElement element = await Window.GetElement("/TextBlock");

        Color background = await element.GetEffectiveBackground();

        Assert.AreEqual(Colors.Red, background);
    }

    [TestMethod]
    public async Task OnGetEffectiveBackground_StopsProcessingAtDefinedParent()
    {
        await Window.SetXamlContent(@"
<Grid Background=""#DDFF0000"">
    <TextBlock />
</Grid>
");
        await Window.SetBackgroundColor(Colors.Blue);

        IVisualElement child = await Window.GetElement("/TextBlock");
        IVisualElement parent = await Window.GetElement("/Grid");

        Color background = await child.GetEffectiveBackground(parent);

        Assert.AreEqual(Color.FromArgb(0xDD, 0xFF, 0x00, 0x00), background);
    }

    [TestMethod]
    public async Task OnGetEffectiveBackground_AppliesOpacityFromParents()
    {
        await Window.SetXamlContent(@"
<Grid Background=""Red"" Opacity=""0.5"" x:Name=""RedGrid"">
    <Grid Background=""Blue"" x:Name=""BlueGrid"">
        <TextBlock />
    </Grid>
</Grid>
");
        await Window.SetBackgroundColor(Colors.Lime);

        IVisualElement child = await Window.GetElement("/TextBlock");
        IVisualElement parent = await Window.GetElement("BlueGrid");

        Color background = await child.GetEffectiveBackground(parent);

        Assert.AreEqual(Color.FromArgb(127, 0x00, 0x00, 0xFF), background);
    }
}
