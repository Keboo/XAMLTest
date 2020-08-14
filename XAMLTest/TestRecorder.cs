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
        private string BaseFileName { get; }
        private string Directory { get; }

        private bool IsDisposed { get; set; }
        private bool IsSuccess { get; set; }

        public TestRecorder(IApp app,
            [CallerFilePath] string callerFilePath = "",
            [CallerMemberName] string unitTestMethod = "")
        {
            App = app ?? throw new ArgumentNullException(nameof(app));

            //"C:\\Dev\\MaterialDesignInXamlToolkit\\MaterialDesignThemes.UITests\\WPF\\TextBox\\TextBoxTests.cs"
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
            var rootDirectory = Path.GetDirectoryName(Assembly.GetCallingAssembly().Location) ?? Path.GetFullPath(".");
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

        public async Task SaveScreenshot([CallerLineNumber] int? lineNumber = null)
            => await SaveScreenshot(lineNumber?.ToString() ?? "");

        private async Task SaveScreenshot(string suffix)
        {
            int index = 1;
            foreach (IWindow window in await App.GetWindows())
            {
                string fileName = $"{BaseFileName}{suffix}-win{index++}.jpg";
                string fullPath = Path.Combine(Directory, fileName);
                File.Delete(fullPath);

                if (await window.GetBitmap() is IImage screenshot)
                {
                    await screenshot.Save(fullPath);
                }
            }
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
