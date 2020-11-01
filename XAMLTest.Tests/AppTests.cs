using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using XamlTest.Tests.TestControls;
using XamlTest.Transport;

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

        [TestMethod]
        [ExpectedException(typeof(Exception))]
        public async Task OnGetResource_ThorwsExceptionWhenNotFound()
        {
            await using var app = App.StartRemote();

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            await app.GetResource("TestResource");
        }

        [TestMethod]
        public async Task OnGetResource_ReturnsFoundResource()
        {
            await using var app = App.StartRemote();

            await app.InitializeWithResources(
                "<Color x:Key=\"TestResource\">Red</Color>",
                Assembly.GetExecutingAssembly().Location);

            IResource resource = await app.GetResource("TestResource");

            Assert.AreEqual("TestResource", resource.Key);
            Assert.AreEqual(Colors.Red.ToString(), resource.Value);
            Assert.AreEqual(typeof(Color).AssemblyQualifiedName, resource.ValueType);
        }

        [TestMethod]
        public async Task OnRegisterSerializer_RegistersCustomSerializer()
        {
            await using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
            IWindow window = await app.CreateWindowWithContent("", title: "Test Window Title");

            await app.RegisterSerializer<CustomSerializer>();

            Assert.AreEqual("In-Test Window Title-Out", await window.GetTitle());

            recorder.Success();
        }

        [TestMethod]
        public async Task OnGetSerializers_ReturnsDefaultSerializers()
        {
            await using var app = App.StartRemote();
            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            var serializers = await app.GetSerializers();

            Assert.AreEqual(2, serializers.Count);
            Assert.IsInstanceOfType(serializers[0], typeof(SolidColorBrushSerializer));
            Assert.IsInstanceOfType(serializers[1], typeof(DefaultSerializer));
        }

        [TestMethod]
        public async Task OnGetSerializers_IncludesCustomSerializers()
        {
            await using var app = App.StartRemote();
            await app.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            await app.RegisterSerializer<CustomSerializer>(1);

            var serializers = await app.GetSerializers();

            Assert.AreEqual(3, serializers.Count);
            Assert.IsInstanceOfType(serializers[0], typeof(SolidColorBrushSerializer));
            Assert.IsInstanceOfType(serializers[1], typeof(CustomSerializer));
            Assert.IsInstanceOfType(serializers[2], typeof(DefaultSerializer));
        }

        private class CustomSerializer : ISerializer
        {
            public bool CanSerialize(Type type) => type == typeof(string);

            public object? Deserialize(Type type, string value)
            {
                return $"{value}-Out";
            }

            public string Serialize(Type type, object? value)
            {
                return $"In-{value}";
            }
        }
    }
}
