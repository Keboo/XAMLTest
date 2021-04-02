using System;
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
        public string Directory { get; }

        private bool IsDisposed { get; set; }
        public bool IsSuccess { get; private set; }

        public TestRecorder(IApp app,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string unitTestMethod = "")
        {
            App = app ?? throw new ArgumentNullException(nameof(app));

            var callingAssembly = Assembly.GetCallingAssembly();
            var assemblyName = callingAssembly.GetName().Name!;
            int assemblyNameIndex = callerFilePath.IndexOf(assemblyName);
            if (assemblyNameIndex >= 0)
            {
                Directory = callerFilePath[(assemblyNameIndex + assemblyName.Length + 1)..];
            }
            else
            {
                Directory = Path.GetFileName(callerFilePath);
            }
            Directory = Path.ChangeExtension(Directory, "").TrimEnd('.');
            var rootDirectory = Path.GetDirectoryName(callingAssembly.Location) ?? Path.GetFullPath(".");
            Directory = Path.Combine(rootDirectory, "Screenshots", Directory);

            System.IO.Directory.CreateDirectory(Directory);

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
            string fullPath = Path.Combine(Directory, fileName);
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
