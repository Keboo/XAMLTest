using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
            foreach(var file in GetScreenshots(new TestRecorder(new Simulators.App())))
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
            var app = new Simulators.App();
            TestRecorder testRecorder = new(app);

            Assert.IsNotNull(await testRecorder.SaveScreenshot());

            var file = GetScreenshots(testRecorder).Single();

            TestContext.WriteLine($"Full path {file}");
            var fileName = Path.GetFileName(file);
            Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
            Assert.AreEqual($"{nameof(SaveScreenshot_SavesImage)}{GetLineNumber(-7)}-win1.jpg", fileName);
        }

        [TestMethod]
        public async Task SaveScreenshot_WithSuffix_SavesImage()
        {
            var app = new Simulators.App();
            TestRecorder testRecorder = new(app);

            Assert.IsNotNull(await testRecorder.SaveScreenshot("MySuffix"));

            var file = GetScreenshots(testRecorder).Single();

            TestContext.WriteLine($"Full path {file}");
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
