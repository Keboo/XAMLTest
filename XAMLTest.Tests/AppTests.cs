using MaterialDesignThemes.Wpf;
using XamlTest;
using XamlTest.Tests.TestControls;

[assembly: GenerateHelpers(typeof(ColorZone))]
namespace XamlTest.Tests;

[TestClass]
public class AppTests
{
    [NotNull]
    public TestContext? TestContext { get; set; }
    
    [TestMethod]
    public async Task OnStartRemote_LaunchesRemoteApp()
    {
        await using var app = await App.StartRemote<XAMLTest.TestApp.App>(TestContext.WriteLine);
        await using var recorder = new TestRecorder(app);
        IWindow? window = await app.GetMainWindow();
        Assert.AreEqual("Test App Window", await window!.GetTitle());

        recorder.Success();
    }

    [TestMethod]
    public async Task CanGenerateTypedElement_ForCustomControlInRemoteApp()
    {
        await using var app = await App.StartRemote<XAMLTest.TestApp.App>(TestContext.WriteLine);
        await using var recorder = new TestRecorder(app);
        IWindow? window = await app.GetMainWindow();
        Assert.IsNotNull(window);

        IVisualElement<ColorZone> colorZone = await window.GetElement<ColorZone>("/ColorZone");

        Assert.AreEqual(ColorZoneMode.PrimaryMid, await colorZone.GetMode());

        recorder.Success();
    }

    [TestMethod]
    public async Task CanGenerateTypedElement_ForCustomControlInXaml()
    {
        await using var app = await App.StartRemote<XAMLTest.TestApp.App>(TestContext.WriteLine);
        await using var recorder = new TestRecorder(app);
        app.DefaultXmlNamespaces.Add("materialDesign", "http://materialdesigninxaml.net/winfx/xaml/themes");
        IWindow? window = await app.GetMainWindow();
        Assert.IsNotNull(window);

        IVisualElement<ColorZone> colorZone = await window.SetXamlContent<ColorZone>(@"
<materialDesign:ColorZone Mode=""PrimaryLight"">
    <TextBlock Text=""Test Header"" x:Name=""Header"" HorizontalAlignment=""Center"" Margin=""0,15"" />
</materialDesign:ColorZone>");

        Assert.AreEqual(ColorZoneMode.PrimaryLight, await colorZone.GetMode());

        recorder.Success();
    }

    [TestMethod]
    public async Task OnCreateWindow_CanReadTitle()
    {
        await using var app = await App.StartRemote(TestContext.WriteLine);
        await using var recorder = new TestRecorder(app);

        await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
        IWindow window = await app.CreateWindowWithContent("", title: "Test Window Title");

        Assert.AreEqual("Test Window Title", await window.GetTitle());

        recorder.Success();
    }

    [TestMethod]
    public async Task OnCreateWindow_CanUseCustomWindow()
    {
        await using var app = await App.StartRemote(TestContext.WriteLine);
        await using var recorder = new TestRecorder(app);

        await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        IWindow window = await app.CreateWindow<TestWindow>();

        Assert.AreEqual("Custom Test Window", await window.GetTitle());

        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetMainWindow_ReturnsNullBeforeWindowCreated()
    {
        await using var app = await App.StartRemote(TestContext.WriteLine);
        await using var recorder = new TestRecorder(app);

        await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        IWindow? mainWindow = await app.GetMainWindow();

        Assert.IsNull(mainWindow);

        recorder.Success();
    }

    [TestMethod]
    public async Task OnGetMainWindow_AfterMainWindowShownReturnsMainWindow()
    {
        await using var app = await App.StartRemote(TestContext.WriteLine);
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
        await using var app = await App.StartRemote(TestContext.WriteLine);
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
        await using var app = await App.StartRemote<XAMLTest.TestApp.App>(TestContext.WriteLine);
        await using var recorder = new TestRecorder(app);

        IWindow? window = await app.GetMainWindow();

        Assert.IsNotNull(window);
        object? tag = await window.GetTag();

        Assert.IsTrue(tag?.ToString()?.Contains("--debug"));
    }

    [TestMethod]
    [Ignore("This test only handle Win32 apps, not anything with CoreWindow")]
    public async Task OnStartWithMinimizeOtherWindows_MinimizesWindows()
    {
        Process? notepadProcess = null;
        try
        {
            notepadProcess = Process.Start("notepad.exe");
            await Wait.For(() => 
            {
                notepadProcess.Refresh();
                if (notepadProcess.HasExited)
                {
                    notepadProcess = Process.GetProcesses()
                        .Where(x => string.Equals(x.ProcessName, "Notepad", StringComparison.InvariantCultureIgnoreCase))
                        .FirstOrDefault();
                }
                return Task.FromResult(notepadProcess?.MainWindowHandle is { } handle && handle != IntPtr.Zero);
            }, new Retry(10, TimeSpan.FromSeconds(10)));
            IntPtr hWnd = notepadProcess.MainWindowHandle;
            Assert.AreNotEqual(IntPtr.Zero, hWnd);

            PInvoke.User32.WindowShowStyle windowState = PInvoke.User32.GetWindowPlacement(hWnd).showCmd;
            Assert.AreNotEqual(PInvoke.User32.WindowShowStyle.SW_SHOWMINIMIZED, windowState);

            await using var app = await App.StartRemote(new AppOptions<XAMLTest.TestApp.App>
            {
                LogMessage = TestContext.WriteLine,
                MinimizeOtherWindows = true
            });

            IWindow? window = await app.GetMainWindow();
            Assert.IsNotNull(window);

            windowState = PInvoke.User32.GetWindowPlacement(hWnd).showCmd;
            Assert.AreEqual(PInvoke.User32.WindowShowStyle.SW_SHOWMINIMIZED, windowState);
        }
        finally
        {
            notepadProcess?.Kill();
        }
    }
}
