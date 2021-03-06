﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
        [NotNull]
        private static IApp? App { get; set; }

        [NotNull]
        private static IWindow? Window { get; set; }

        [ClassInitialize]
        public static async Task ClassInitialize(TestContext context)
        {
            App = XamlTest.App.StartRemote(logMessage: msg => context.WriteLine(msg));

            await App.InitializeWithDefaults(Assembly.GetExecutingAssembly().Location);

            Window = await App.CreateWindowWithContent(@"<Border />");
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            App.Dispose();
        }

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
            await using IApp app = new Simulators.App();
            TestRecorder testRecorder = new(app);

            Assert.IsNotNull(await testRecorder.SaveScreenshot());

            string? file = GetScreenshots(testRecorder).Single();

            string? fileName = Path.GetFileName(file);
            Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
            Assert.AreEqual($"{nameof(SaveScreenshot_SavesImage)}{GetLineNumber(-6)}-win1.jpg", fileName);
        }

        [TestMethod]
        public async Task SaveScreenshot_WithSuffix_SavesImage()
        {
            await using var app = new Simulators.App();
            TestRecorder testRecorder = new(app);

            Assert.IsNotNull(await testRecorder.SaveScreenshot("MySuffix"));

            var file = GetScreenshots(testRecorder).Single();

            var fileName = Path.GetFileName(file);
            Assert.AreEqual(nameof(TestRecorderTests), Path.GetFileName(Path.GetDirectoryName(file)));
            Assert.AreEqual($"{nameof(SaveScreenshot_WithSuffix_SavesImage)}MySuffix{GetLineNumber(-6)}-win1.jpg", fileName);
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
