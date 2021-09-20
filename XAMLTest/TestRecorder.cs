﻿using System;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace XamlTest
{
    public sealed class TestRecorder : IAsyncDisposable
    {
        public IApp App { get; }
        public string BaseFileName { get; }

        private bool IsDisposed { get; set; }
        public bool IsSuccess { get; private set; }

        private static object SyncLock { get; } = new object();
        private Lazy<string> Directory { get; }

        public TestRecorder(IApp app,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string unitTestMethod = "")
        {
            App = app ?? throw new ArgumentNullException(nameof(app));

            Directory = new Lazy<string>(() =>
            {
                lock(SyncLock)
                {
                    var callingAssembly = Assembly.GetCallingAssembly();
                    var assemblyName = callingAssembly.GetName().Name!;
                    int assemblyNameIndex = callerFilePath.IndexOf(assemblyName);
                    string directory;
                    if (assemblyNameIndex >= 0)
                    {
                        directory = callerFilePath[(assemblyNameIndex + assemblyName.Length + 1)..];
                    }
                    else
                    {
                        directory = Path.GetFileName(callerFilePath);
                    }
                    directory = Path.ChangeExtension(directory, "").TrimEnd('.');
                    var rootDirectory = Path.GetDirectoryName(callingAssembly.Location) ?? Path.GetFullPath(".");
                    directory = Path.Combine(rootDirectory, "Screenshots", directory);

                    System.IO.Directory.CreateDirectory(directory);
                    return directory;
                }
            });

            BaseFileName = unitTestMethod;
            foreach (char invalidChar in Path.GetInvalidFileNameChars())
            {
                BaseFileName = BaseFileName.Replace($"{invalidChar}", "");
            }
        }

        /// <summary>
        /// Calling this method indicates that the test completed successfully and no additional recording is needed.
        /// </summary>
        public void Success() => IsSuccess = true;

        public async Task<string?> SaveScreenshot([CallerLineNumber] int? lineNumber = null)
            => await SaveScreenshot(lineNumber?.ToString() ?? "");

        public async Task<string?> SaveScreenshot(string suffix, [CallerLineNumber] int? lineNumber = null)
            => await SaveScreenshot($"{suffix}{lineNumber?.ToString() ?? ""}");

        private async Task<string?> SaveScreenshot(string suffix)
        {
            int index = 1;
            string fileName = $"{BaseFileName}{suffix}-win{index++}.jpg";
            string fullPath = Path.Combine(Directory.Value, fileName);
            File.Delete(fullPath);

            if (await App.GetScreenshot() is IImage screenshot)
            {
                await screenshot.Save(fullPath);
                return fullPath;
            }
            return null;
        }

        #region IDisposable Support
        private async ValueTask DisposeAsync(bool disposing)
        {
            if (!IsDisposed)
            {
                if (disposing)
                {
                    if (!IsSuccess)
                    {
                        await SaveScreenshot("");
                    }
                }
                IsDisposed = true;
            }
        }

        public ValueTask DisposeAsync() => DisposeAsync(true);
        #endregion

    }
}
