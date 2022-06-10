using System.Diagnostics;
using XamlTest.Tests.TestControls;

#if WPF
using MaterialDesignThemes.Wpf;
[assembly: GenerateHelpers(typeof(ColorZone))]
#endif
namespace XamlTest.Tests;

[TestClass]
public class AppTests
{
    [TestMethod]
    public async Task OnStartRemote_LaunchesRemoteApp()
    {
        await using var app = await App.StartRemote<XAMLTest.TestApp.App>();
        IWindow? window = await app.GetMainWindow();
        Assert.AreEqual("Test App Window", await window!.GetTitle());
    }

#if WPF
    [TestMethod]
    public async Task CanGenerateTypedElement_ForCustomControlInRemoteApp()
    {
        await using var app = await App.StartRemote<XAMLTest.TestApp.App>();
        IWindow? window = await app.GetMainWindow();
        Assert.IsNotNull(window);

        IVisualElement<ColorZone> colorZone = await window.GetElement<ColorZone>("/ColorZone");

        Assert.AreEqual(ColorZoneMode.PrimaryMid, await colorZone.GetMode());
    }

    [TestMethod]
    public async Task CanGenerateTypedElement_ForCustomControlInXaml()
    {
        await using var app = await App.StartRemote<XAMLTest.TestApp.App>();
        app.DefaultXmlNamespaces.Add("materialDesign", "http://materialdesigninxaml.net/winfx/xaml/themes");
        IWindow? window = await app.GetMainWindow();
        Assert.IsNotNull(window);

        IVisualElement<ColorZone> colorZone = await window.SetXamlContent<ColorZone>(@"
<materialDesign:ColorZone Mode=""PrimaryLight"">
    <TextBlock Text=""Test Header"" x:Name=""Header"" HorizontalAlignment=""Center"" Margin=""0,15"" />
</materialDesign:ColorZone>");

        Assert.AreEqual(ColorZoneMode.PrimaryLight, await colorZone.GetMode());
    }
#endif

    [TestMethod]
    public async Task OnCreateWindow_CanReadTitle()
    {
        await using var app = await App.StartRemote();
        await using var recorder = new TestRecorder(app);

        await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
        IWindow window = await app.CreateWindowWithContent("", title: "Test Window Title");

        Assert.AreEqual("Test Window Title", await window.GetTitle());

        recorder.Success();
    }

    [TestMethod]
    public async Task OnCreateWindow_CanUseCustomWindow()
    {
        Debugger.Launch();
        await using var app = await App.StartRemote();
        await using var recorder = new TestRecorder(app);

        await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        IWindow window = await app.CreateWindow<TestWindow>();

        Assert.AreEqual("Custom Test Window", await window.GetTitle());

        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetMainWindow_ReturnsNullBeforeWindowCreated()
    {
        await using var app = await App.StartRemote();
        await using var recorder = new TestRecorder(app);

        await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        IWindow? mainWindow = await app.GetMainWindow();

        Assert.IsNull(mainWindow);

        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetMainWindow_AfterMainWindowShownReturnsMainWindow()
    {
        await using var app = await App.StartRemote();
        await using var recorder = new TestRecorder(app);

        await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        IWindow window1 = await app.CreateWindowWithContent("");
        IWindow window2 = await app.CreateWindowWithContent("");

        IWindow? mainWindow = await app.GetMainWindow();

        Assert.AreEqual(window1, mainWindow);

        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetWindows_ReturnsAllWindows()
    {
        await using var app = await App.StartRemote();
        await using var recorder = new TestRecorder(app);

        await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        IWindow window1 = await app.CreateWindowWithContent("");
        IWindow window2 = await app.CreateWindowWithContent("");

        IReadOnlyList<IWindow> windows = await app.GetWindows();

        CollectionAssert.AreEqual(new[] { window1, window2 }, windows.ToArray());

        recorder.Success();
    }

    [TestMethod]
    public async Task OnStartWithDebugger_LaunchesWithDebugFlag()
    {
        if (!Debugger.IsAttached)
        {
            Assert.Inconclusive("This test must be run with a debugger attached");
        }
        await using var app = await App.StartRemote<XAMLTest.TestApp.App>();
        IWindow? window = await app.GetMainWindow();

        Assert.IsNotNull(window);
        string? commandLine = await window.GetProperty<string>("CommandLine");

        Assert.IsTrue(commandLine?.Contains("--debug"));
    }
}
