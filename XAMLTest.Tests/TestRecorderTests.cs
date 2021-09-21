using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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
        [NotNull]
        public TestContext? TestContext { get; set; }

        [ClassInitialize]
        public static void ClassInit(TestContext _)
        {
            foreach (var file in new TestRecorder(new Simulators.App()).EnumerateScreenshots())
            {
                File.Delete(file);
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (var file in new TestRecorder(new Simulators.App()).EnumerateScreenshots())
            {
                File.Delete(file);
            }
        }

        [TestMethod]
        public async Task SaveScreenshot_SavesImage()
        {
            await using IApp app = new Simulators.App();
            await using TestRecorder testRecorder = new(app);

            Assert.IsNotNull(await testRecorder.SaveScreenshot());

            string? file = testRecorder.EnumerateScreenshots()
                .Where(x => Path.GetFileName(Path.GetDirectoryName(x)) == nameof(TestRecorderTests) &&
                       Path.GetFileName(x).StartsWith(nameof(SaveScreenshot_SavesImage)))
                .Single();

            string? fileName = Path.GetFileName(file);
            Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
            Assert.AreEqual($"{nameof(SaveScreenshot_SavesImage)}{GetLineNumber(-9)}-win1.jpg", fileName);
            testRecorder.Success();
        }

        [TestMethod]
        public async Task SaveScreenshot_WithSuffix_SavesImage()
        {
            await using var app = new Simulators.App();
            await using TestRecorder testRecorder = new(app);

            Assert.IsNotNull(await testRecorder.SaveScreenshot("MySuffix"));

            var file = testRecorder.EnumerateScreenshots()
                .Where(x => Path.GetFileName(Path.GetDirectoryName(x)) == nameof(TestRecorderTests) &&
                       Path.GetFileName(x).StartsWith(nameof(SaveScreenshot_WithSuffix_SavesImage)))
                .Single();

            var fileName = Path.GetFileName(file);
            Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
            Assert.AreEqual($"{nameof(SaveScreenshot_WithSuffix_SavesImage)}MySuffix{GetLineNumber(-9)}-win1.jpg", fileName);
            testRecorder.Success();
        }

        [TestMethod, ExpectedException(typeof(NotImplementedException))]
        public async Task TestRecord_WhenExceptionThrown_DoesNotRethrow()
        {
            await using var app = new Simulators.App();
            await using TestRecorder testRecorder = new(app);
            await app.InitializeWithDefaults(null!);
        }

        private static int GetLineNumber(int offset = 0, [CallerLineNumber] int lineNumber = 0)
            => lineNumber + offset;
    }
}
