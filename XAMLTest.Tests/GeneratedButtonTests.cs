using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace XamlTest.Tests
{
    [TestClass]
    public class GeneratedButtonTests
    {
        public TestContext TestContext { get; set; } = null!;

        [NotNull]
        private IApp? App { get; set; }

        [TestInitialize]
        public async Task TestInitialize()
        {
            App = XamlTest.App.StartRemote(logMessage: msg => TestContext.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);
        }

        [TestCleanup]
        public void TestCleanup()
        {
            App.Dispose();
        }

        //Generate tests
        [TestMethod]
        public async Task CanInvoke_IsDefault_ReturnsValue()
        {
            // Arrange
            await using TestRecorder recorder = new(App);

            IWindow window = await App.CreateWindowWithContent(@$"<Button x:Name=""TestButton"" />");

            //Act
            IVisualElement<Button> button = await window.GetElement<Button>("TestButton");

            //Assert
            Assert.AreEqual(false, await button.GetIsDefault());

            recorder.Success();
        }
    }
}
