using MaterialDesignThemes.Wpf;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using XamlTest;
using XamlTest.Tests.TestControls;

[assembly: GenerateHelpers(typeof(ColorZone))]


namespace XamlTest.Tests
{
    [TestClass]
    public class AppTests
    {
        [TestMethod]
        public async Task OnStartRemote_LaunchesRemoteApp()
        {
            await using var app = App.StartRemote<XAMLTest.TestApp.App>();
            IWindow? window = await app.GetMainWindow();
            Assert.AreEqual("Test App Window", await window!.GetTitle());
        }

        [TestMethod]
        public async Task CanGenerateTypedElement_ForCustomControlInRemoteApp()
        {
            await using var app = App.StartRemote<XAMLTest.TestApp.App>();
            IWindow? window = await app.GetMainWindow();
            Assert.IsNotNull(window);

            IVisualElement<ColorZone> colorZone = await window.GetElement<ColorZone>("/ColorZone");

            Assert.AreEqual(ColorZoneMode.PrimaryMid, await colorZone.GetMode());
        }

        [TestMethod]
        public async Task OnCreateWindow_CanReadTitle()
        {
            await using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
            IWindow window = await app.CreateWindowWithContent("", title: "Test Window Title");

            Assert.AreEqual("Test Window Title", await window.GetTitle());

            recorder.Success();
        }

        [TestMethod]
        public async Task OnCreateWindow_CanUseCustomWindow()
        {
            await using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            IWindow window = await app.CreateWindow<TestWindow>();

            Assert.AreEqual("Custom Test Window", await window.GetTitle());

            recorder.Success();
        }

        [TestMethod]
        public async Task OnGetMainWindow_ReturnsNullBeforeWindowCreated()
        {
            await using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            IWindow? mainWindow = await app.GetMainWindow();

            Assert.IsNull(mainWindow);

            recorder.Success();
        }

        [TestMethod]
        public async Task OnGetMainWindow_AfterMainWindowShownReturnsMainWindow()
        {
            await using var app = App.StartRemote();
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
            await using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            IWindow window1 = await app.CreateWindowWithContent("");
            IWindow window2 = await app.CreateWindowWithContent("");

            IReadOnlyList<IWindow> windows = await app.GetWindows();

            CollectionAssert.AreEqual(new[] { window1, window2 }, windows.ToArray());

            recorder.Success();
        }
    }
}
