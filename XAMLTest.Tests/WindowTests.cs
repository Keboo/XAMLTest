using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;

namespace XamlTest.Tests
{
    [TestClass]
    public class WindowTests
    {
        [TestMethod]
        public async Task TestMethod1()
        {
            using var app = App.StartRemote();
            await using var recorder = new TestRecorder(app);

            await app.Initialize(@"<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""/>", Assembly.GetExecutingAssembly().Location);

            string xaml = @$"<Window
        xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
        xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
        mc:Ignorable=""d""
        Height=""800"" Width=""1100""
        Title=""Test Window""
        WindowStartupLocation=""CenterScreen"">
</Window>";
            IWindow window = await app.CreateWindow(xaml);

            Assert.AreEqual("Test Window", await window.GetTitle());
            await window.WaitForLoaded();

            await recorder.SaveScreenshot();

            recorder.Success();
        }
    }
}
