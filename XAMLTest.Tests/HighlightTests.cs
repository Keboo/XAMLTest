using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

namespace XamlTest.Tests;

[TestClass]
public class HighlightTests
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

    [TestMethod]
    public async Task OnHighlight_WithDefaults_AddsHighlightAdorner()
    {
        await Window.SetXamlContent(@"<Grid x:Name=""MyGrid"" Margin=""50"" />");

        IVisualElement<Grid> grid = await Window.GetElement<Grid>("MyGrid");

        await grid.Highlight();

        IVisualElement<Adorner> adorner = await grid.GetElement<Adorner>("/SelectionAdorner");

        Assert.IsNotNull(adorner);
        Assert.AreEqual(HighlightConfig.DefaultBorderColor, await adorner.GetProperty<Color>("BorderBrush"));
        Assert.AreEqual(HighlightConfig.DefaultBorderWidth, await adorner.GetProperty<double>("BorderThickness"));
        Assert.AreEqual(HighlightConfig.DefaultOverlayColor, await adorner.GetProperty<Color>("OverlayBrush"));
    }

    [TestMethod]
    public async Task OnHighlight_WithCustomValues_AddsHighlightAdorner()
    {
        await Window.SetXamlContent(@"<Grid x:Name=""MyGrid"" Margin=""50"" />");

        IVisualElement<Grid> grid = await Window.GetElement<Grid>("MyGrid");

        await grid.Highlight(new HighlightConfig()
        {
            BorderBrush = new SolidColorBrush(Colors.Blue),
            BorderThickness = 3,
            OverlayBrush = new SolidColorBrush(Colors.Green)
        });

        IVisualElement<Adorner> adorner = await grid.GetElement<Adorner>("/SelectionAdorner");

        Assert.IsNotNull(adorner);
        Assert.AreEqual(Colors.Blue, await adorner.GetProperty<Color>("BorderBrush"));
        Assert.AreEqual(3.0, await adorner.GetProperty<double>("BorderThickness"));
        Assert.AreEqual(Colors.Green, await adorner.GetProperty<Color>("OverlayBrush"));
    }

    [TestMethod]
    public async Task OnHighlight_WithExistingHighlight_UpdatesHighlight()
    {
        await Window.SetXamlContent(@"<Grid x:Name=""MyGrid"" Margin=""50"" />");

        IVisualElement<Grid> grid = await Window.GetElement<Grid>("MyGrid");

        await grid.Highlight();
        await grid.Highlight(new HighlightConfig()
        {
            BorderBrush = new SolidColorBrush(Colors.Blue),
            BorderThickness = 3,
            OverlayBrush = new SolidColorBrush(Colors.Green)
        });

        IVisualElement<Adorner> adorner = await grid.GetElement<Adorner>("/SelectionAdorner");

        Assert.IsNotNull(adorner);
        Assert.AreEqual(Colors.Blue, await adorner.GetProperty<Color>("BorderBrush"));
        Assert.AreEqual(3.0, await adorner.GetProperty<double>("BorderThickness"));
        Assert.AreEqual(Colors.Green, await adorner.GetProperty<Color>("OverlayBrush"));
    }

    [TestMethod]
    public async Task OnClearHighlight_WithHighlight_ClearsAdorner()
    {
        await Window.SetXamlContent(@"<Grid x:Name=""MyGrid"" Margin=""50"" />");

        IVisualElement<Grid> grid = await Window.GetElement<Grid>("MyGrid");

        await grid.Highlight();
        await grid.ClearHighlight();

        var ex = await Assert.ThrowsExceptionAsync<XamlTestException>(() => grid.GetElement<Adorner>("/SelectionAdorner"));

        Assert.IsTrue(ex.Message.Contains("Failed to find child element of type 'SelectionAdorner'"));
    }
}
