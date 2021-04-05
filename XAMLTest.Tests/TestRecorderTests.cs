using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace XamlTest.Tests
{
    [TestClass]
    public class TestRecorderTests
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
        public TestContext TestContext { get; set; }
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.

        [TestInitialize]
        public void TestInit()
        {
            foreach (var file in GetScreenshots(new TestRecorder(new Simulators.App())))
            {
                File.Delete(file);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            foreach (var file in GetScreenshots(new TestRecorder(new Simulators.App())))
            {
                File.Delete(file);
            }
        }

        [TestMethod]
        public async Task SaveScreenshot_SavesImage()
        {
            IApp app = new Simulators.App();
            var testRecorder = new TestRecorder(app);

            Assert.IsNotNull(await testRecorder.SaveScreenshot());

            string? file = GetScreenshots(testRecorder).Single();

            string? fileName = Path.GetFileName(file);
            Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
            Assert.AreEqual($"{nameof(SaveScreenshot_SavesImage)}{GetLineNumber(-4)}-win1.jpg", fileName);
        }

        [TestMethod]
        public async Task SaveScreenshot_WithSuffix_SavesImage()
        {
            var app = new Simulators.App();
            var testRecorder = new TestRecorder(app);

            Assert.IsNotNull(await testRecorder.SaveScreenshot("MySuffix"));

            var file = GetScreenshots(testRecorder).Single();

            var fileName = Path.GetFileName(file);
            Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
            Assert.AreEqual($"{nameof(SaveScreenshot_WithSuffix_SavesImage)}MySuffix{GetLineNumber(-7)}-win1.jpg", fileName);
        }

        private static int GetLineNumber(int offset = 0, [CallerLineNumber] int lineNumber = 0)
            => lineNumber + offset;

        private static IEnumerable<string> GetScreenshots(
            TestRecorder testRecorder)
        {
            if (!Directory.Exists(testRecorder.Directory))
            {
                return Array.Empty<string>();
            }
            return Directory.EnumerateFiles(testRecorder.Directory, "*.jpg", SearchOption.AllDirectories);
        }
    }
}
