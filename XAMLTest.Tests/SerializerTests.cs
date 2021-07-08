using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Media;
using XamlTest.Transport;

namespace XamlTest.Tests
{
    [TestClass]
    public class SerializerTests
    {
        [NotNull]
        private static IApp? App { get; set; }

        [NotNull]
        private static IWindow? Window { get; set; }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            App = XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            Window = await App.CreateWindowWithContent("", title: "Test Window Title");
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            App.Dispose();
        }

        [TestMethod]
        public async Task OnRegisterSerializer_RegistersCustomSerializer()
        {
            await using var recorder = new TestRecorder(App);

            await App.RegisterSerializer<CustomSerializer>();

            Assert.AreEqual("In-Test Window Title-Out", await Window.GetTitle());

            recorder.Success();
        }

        [TestMethod]
        public async Task OnGetSerializers_ReturnsDefaultSerializers()
        {
            var serializers = await App.GetSerializers();

            Assert.AreEqual(5, serializers.Count);
            Assert.IsInstanceOfType(serializers[0], typeof(BrushSerializer));
            Assert.IsInstanceOfType(serializers[1], typeof(CharSerializer));
            Assert.IsInstanceOfType(serializers[2], typeof(GridSerializer));
            Assert.IsInstanceOfType(serializers[3], typeof(SecureStringSerializer));
            Assert.IsInstanceOfType(serializers[4], typeof(DefaultSerializer));
        }

        [TestMethod]
        public async Task OnGetSerializers_IncludesCustomSerializers()
        {
            var initialSerializersCount = (await App.GetSerializers()).Count;

            await App.RegisterSerializer<CustomSerializer>(1);

            var serializers = await App.GetSerializers();

            Assert.AreEqual(initialSerializersCount + 1, serializers.Count);
            Assert.IsInstanceOfType(serializers[1], typeof(CustomSerializer));
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
