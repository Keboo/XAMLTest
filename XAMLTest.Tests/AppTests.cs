using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;

namespace XamlTest.Tests
{
    [TestClass]
    public class AppTests
    {
        public TestContext TestContext { get; set; }

        [TestMethod]
        public async Task OnCreateWindow_CanReadTitle()
        {
            using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            IWindow window = await app.CreateWindowWithContent("", title: "Test Window Title");

            Assert.AreEqual("Test Window Title", await window.GetTitle());

            recorder.Success();
        }

        [TestMethod]
        public async Task OnGetMainWindow_ReturnsNullBeforeWindowCreated()
        {
            using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            IWindow mainWindow = await app.GetMainWindow();
            
            Assert.IsNull(mainWindow);

            recorder.Success();
        }

        [TestMethod]
        public async Task OnGetMainWindow_AfterMainWindowShownReturnsMainWindow()
        {
            using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            IWindow window1 = await app.CreateWindowWithContent("");
            IWindow window2 = await app.CreateWindowWithContent("");

            IWindow mainWindow = await app.GetMainWindow();
            Assert.AreEqual(window1, mainWindow);

            recorder.Success();
        }
    }
}
