using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Reflection;
using System.Threading.Tasks;

namespace XamlTest.Tests
{
    [TestClass]
    public class WindowTests
    {
        public TestContext Context { get; set; }

        [TestMethod]
        public async Task TestMethod1()
        {
            Context.WriteLine("Starting app");
            using var app = App.StartRemote();
            Context.WriteLine("Setup recorder");
            await using var recorder = new TestRecorder(app);

            Context.WriteLine("Initialize app");
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
            Context.WriteLine("Create window");
            IWindow window = await app.CreateWindow(xaml);

            Assert.AreEqual("Test Window", await window.GetTitle());
            Context.WriteLine("Wait for loaded");
            await window.WaitForLoaded();

            Context.WriteLine("Save Screenshot");
            await recorder.SaveScreenshot();

            Context.WriteLine("Success");
            recorder.Success();
        }
    }
}
