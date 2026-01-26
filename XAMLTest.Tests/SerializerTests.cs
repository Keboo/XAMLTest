using XamlTest.Transport;

namespace XamlTest.Tests;

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
        App = await XamlTest.App.StartRemote(logMessage: context.WriteLine);

        await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

        Window = await App.CreateWindowWithContent("", title: "Test Window Title");
    }

    [ClassCleanup(Microsoft.VisualStudio.TestTools.UnitTesting.InheritanceBehavior.BeforeEachDerivedClass)]
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
        var serializers = (await App.GetSerializers()).ToList();

        int brushSerializerIndex = serializers.FindIndex(x => x is BrushSerializer);
        int charSerializerIndex = serializers.FindIndex(x => x is CharSerializer);
        int gridSerializerIndex = serializers.FindIndex(x => x is GridSerializer);
        int secureStringSerializerIndex = serializers.FindIndex(x => x is SecureStringSerializer);
        int defaultSerializerIndex = serializers.FindIndex(x => x is DefaultSerializer);

        Assert.IsLessThan(charSerializerIndex, brushSerializerIndex);
        Assert.IsLessThan(gridSerializerIndex, charSerializerIndex);
        Assert.IsLessThan(secureStringSerializerIndex, gridSerializerIndex);
        Assert.IsLessThan(defaultSerializerIndex, secureStringSerializerIndex);
        Assert.AreEqual(serializers.Count - 1, defaultSerializerIndex);
    }

    [TestMethod]
    public async Task OnGetSerializers_IncludesCustomSerializers()
    {
        var initialSerializersCount = (await App.GetSerializers()).Count;

        await App.RegisterSerializer<CustomSerializer>(1);

        var serializers = await App.GetSerializers();

        Assert.HasCount(initialSerializersCount + 1, serializers);
        Assert.IsInstanceOfType(serializers[1], typeof(CustomSerializer));
    }

    private class CustomSerializer : ISerializer
    {
        public bool CanSerialize(Type type, ISerializer rootSerializer) => type == typeof(string);

        public object? Deserialize(Type type, string value, ISerializer rootSerializer)
        {
            return $"{value}-Out";
        }

        public string Serialize(Type type, object? value, ISerializer rootSerializer)
        {
            return $"In-{value}";
        }
    }
}
