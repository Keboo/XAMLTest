using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace XamlTest.Tests
{
    [TestClass]
    public class TestRecorderTests
    {
        [TestInitialize]
        public void TestInit()
        {
            foreach(var file in GetScreenshots())
            {
                File.Delete(file);
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            foreach (var file in GetScreenshots())
            {
                File.Delete(file);
            }
        }

        [TestMethod]
        public async Task SaveScreenshot_SavesImage()
        {
            var app = new Simulators.App();
            TestRecorder testRecorder = new(app);

            Assert.IsTrue(await testRecorder.SaveScreenshot());

            var file = GetScreenshots().Single();

            var fileName = Path.GetFileName(file);
            Assert.AreEqual($"{nameof(SaveScreenshot_SavesImage)}{GetLineNumber(-5)}-win1.jpg", fileName);
        }

        [TestMethod]
        public async Task SaveScreenshot_WithSuffix_SavesImage()
        {
            var app = new Simulators.App();
            TestRecorder testRecorder = new(app);

            Assert.IsTrue(await testRecorder.SaveScreenshot("MySuffix"));

            var file = GetScreenshots().Single();

            var fileName = Path.GetFileName(file);
            Assert.AreEqual($"{nameof(SaveScreenshot_WithSuffix_SavesImage)}MySuffix{GetLineNumber(-5)}-win1.jpg", fileName);
        }

        private static int GetLineNumber(int offset = 0, [CallerLineNumber] int? lineNumber = null)
            => lineNumber.GetValueOrDefault() + offset;

        private static IEnumerable<string> GetScreenshots(
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string unitTestMethod = "")
        {
            string directory;
            var assembly = typeof(TestRecorderTests).Assembly;
            var assemblyName = assembly.GetName().Name;
            int assemblyNameIndex = callerFilePath.IndexOf(assemblyName!, StringComparison.Ordinal);
            if (assemblyNameIndex >= 0)
            {
                directory = callerFilePath[(assemblyNameIndex + assemblyName!.Length + 1)..];
            }
            else
            {
                directory = Path.GetFileName(callerFilePath);
            }
            directory = Path.ChangeExtension(directory, "").TrimEnd('.');
            var rootDirectory = Path.GetDirectoryName(assembly.Location) ?? Path.GetFullPath(".");
            directory = Path.Combine(rootDirectory, "Screenshots", directory);

            var baseFileName = unitTestMethod;
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                baseFileName = baseFileName.Replace($"{invalidChar}", "");
            }

            if (!Directory.Exists(directory))
            {
                return Array.Empty<string>();
            }
            return Directory.EnumerateFiles(directory, "*.jpg", SearchOption.AllDirectories);
        }
    }
}
