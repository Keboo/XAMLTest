using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;

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

        [TestMethod]
        public async Task OnGetWindows_ReturnsAllWindows()
        {
            using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            IWindow window1 = await app.CreateWindowWithContent("");
            IWindow window2 = await app.CreateWindowWithContent("");

            IReadOnlyList<IWindow> windows = await app.GetWindows();
            
            CollectionAssert.AreEqual(new[] { window1, window2 }, windows.ToArray());

            recorder.Success();
        }

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task OnGetResource_ThorwsExceptionWhenNotFound()
        {
            using var app = App.StartRemote();

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            await app.GetResource("TestResource");
        }

        [TestMethod]
        public async Task OnGetResource_ReturnsFoundResource()
        {
            using var app = App.StartRemote();

            await app.InitializeWithResources(
                "<Color x:Key=\"TestResource\">Red</Color>",
                Assembly.GetExecutingAssembly().Location);

            IResource resource = await app.GetResource("TestResource");

            Assert.AreEqual("TestResource", resource.Key);
            Assert.AreEqual(Colors.Red.ToString(), resource.Value);
            Assert.AreEqual(typeof(Color).AssemblyQualifiedName, resource.ValueType);
        }
    }
}
