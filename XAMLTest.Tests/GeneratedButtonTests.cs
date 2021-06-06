using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace XamlTest.Tests
{
    [TestClass]
    public class GeneratedButtonTests
    {
        [NotNull]
        private static IApp? App { get; set; }
        [NotNull]
        private static IWindow? Window { get; set; }

        [ClassInitialize]
        public static async Task TestInitialize(TestContext context)
        {
            App = XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
            Window = await App.CreateWindowWithContent(@$"<Button x:Name=""TestButton"" />");
        }

        [ClassCleanup]
        public static void TestCleanup()
        {
            App.Dispose();
        }

        //Generate tests
        [TestMethod]
        public async Task CanInvoke_IsDefault_ReturnsValue()
        {
            // Arrange
            await using TestRecorder recorder = new(App);

            //Act
            //IVisualElement<System.Windows.Controls.Viewbox> decorator = await Window.GetElement<System.Windows.Controls.Viewbox>("TestDecorator");
            //var actual = await decorator.GetChild();

            IVisualElement<Button> button = await Window.GetElement<Button>("TestButton");

            //Assert
            Assert.AreEqual(false, await button.GetIsDefault());

            recorder.Success();
        }
    }
}
