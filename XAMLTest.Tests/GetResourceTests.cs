using System.Windows.Controls;
using System.Windows.Media;

namespace XamlTest.Tests;

[TestClass]
public class GetResourceTests
{
    [NotNull]
    private static IApp? App { get; set; }

    [NotNull]
    private static IWindow? Window { get; set; }

    [NotNull]
    private static IVisualElement<Grid>? Grid { get; set; }

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);

        await App.InitializeWithResources(@"
<Color x:Key=""TestColor"">Red</Color>
<SolidColorBrush x:Key=""TestBrush"" Color=""#FF0000"" />",
            Assembly.GetExecutingAssembly().Location);

        Window = await App.CreateWindowWithContent(@"<Grid x:Name=""MyGrid"">
  <Grid.Resources>
    <Color x:Key=""GridColorResource"">Red</Color>
  </Grid.Resources>
</Grid>");

        Grid = await Window.GetElement<Grid>("MyGrid");
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
    public async Task OnAppGetResource_ThrowsExceptionWhenNotFound()
    {
        await Assert.ThrowsAsync<XamlTestException>(async () =>
        {
            await App.GetResource("NotFound");
        });
    }

    [TestMethod]
    public async Task OnAppGetResource_ReturnsFoundResource()
    {
        IResource resource = await App.GetResource("TestColor");

        Assert.AreEqual("TestColor", resource.Key);
        Assert.AreEqual(Colors.Red, resource.GetAs<Color>());
        Assert.AreEqual(typeof(Color).AssemblyQualifiedName, resource.ValueType);
    }

    [TestMethod]
    public async Task OnAppGetResource_ReturnsColorForBrushResource()
    {
        IResource resource = await App.GetResource("TestBrush");

        Assert.AreEqual("TestBrush", resource.Key);
        Color? color = resource.GetAs<Color?>();
        Assert.AreEqual(Colors.Red, color);
    }

    [TestMethod]
    public async Task OnVisualElementGetResource_ThrowsExceptionWhenNotFound()
    {
        await Assert.ThrowsAsync<XamlTestException>(async () =>
        {
            await Grid.GetResource("NotFound");
        });
    }

    [TestMethod]
    public async Task OnVisualElementGetResource_ReturnsFoundResource()
    {
        IResource resource = await Grid.GetResource("GridColorResource");

        Assert.AreEqual("GridColorResource", resource.Key);
        Assert.AreEqual(Colors.Red, resource.GetAs<Color>());
        Assert.AreEqual(typeof(Color).AssemblyQualifiedName, resource.ValueType);
    }
}
