using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace XamlTest.Tests
{
    [TestClass]
    public class SendKeyboardInputTests
    {
        [NotNull]
        private static IApp? App { get; set; }

        [NotNull]
        private static IVisualElement<TextBox>? TextBox { get; set; }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            App = XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            var window = await App.CreateWindowWithContent(@$"<TextBox x:Name=""TestTextBox"" /> ");
            TextBox = await window.GetElement<TextBox>("TestTextBox");
        }

        [ClassCleanup]
        public static async Task TestCleanup()
        {
            if (App is { } app)
            {
                await app.DisposeAsync();
                App = null;
            }
        }

        [TestInitialize]
        public async Task TestInitialize()
        {
            await TextBox.SetText("");
        }

        [TestMethod]
        public async Task SendInput_WithStringInput_SetsText()
        {
            await TextBox.SendInput(new KeyboardInput("Some Text"));

            Assert.AreEqual("Some Text", await TextBox.GetText());
        }

        [TestMethod]
        public async Task SendInput_WithFormattableStringInput_SetsText()
        {
            await TextBox.SendKeyboardInput($"Some Text");

            Assert.AreEqual("Some Text", await TextBox.GetText());
        }

        [TestMethod]
        public async Task SendInput_WithFormattableStringWithKeys_SetsText()
        {
            await TextBox.SendKeyboardInput($"Some{Key.Space}Text");

            Assert.AreEqual("Some Text", await TextBox.GetText());
        }
    }
}
