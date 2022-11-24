using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace XamlTest;

public sealed class TestRecorder : IAsyncDisposable
{
    public IApp App { get; }
    public string BaseFileName { get; }

    private bool IsDisposed { get; set; }
    public bool IsSuccess { get; private set; }

    private string Directory { get; }

    private string TestSuffix { get; }

    private int _imageIndex = 0;

    public TestRecorder(IApp app,
        string? suffix = null,
        [CallerFilePath] string callerFilePath = "",
        [CallerMemberName] string unitTestMethod = "")
    {
        App = app ?? throw new ArgumentNullException(nameof(app));
        TestSuffix = suffix ?? "";

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
        Directory = Path.Combine(rootDirectory, "Screenshots", directory);

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

    /// <summary>
    /// Enumerate all screenshots
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> EnumerateScreenshots()
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            return Enumerable.Empty<string>();
        }
        return System.IO.Directory.EnumerateFiles(Directory, "*.jpg", SearchOption.AllDirectories);
    }

    public async Task<string?> SaveScreenshot([CallerLineNumber] int? lineNumber = null)
        => await SaveScreenshot(lineNumber?.ToString() ?? "");

    public async Task<string?> SaveScreenshot(string suffix, [CallerLineNumber] int? lineNumber = null)
        => await SaveScreenshot($"{suffix}{lineNumber?.ToString() ?? ""}");

    private async Task<string?> SaveScreenshot(string suffix)
    {
        string fileName = $"{BaseFileName}{TestSuffix}{suffix}-{Interlocked.Increment(ref _imageIndex)}.jpg";
        System.IO.Directory.CreateDirectory(Directory);
        string fullPath = Path.Combine(Directory, fileName);
        File.Delete(fullPath);

        try
        {
            if (await App.GetScreenshot() is IImage screenshot)
            {
                await screenshot.Save(fullPath);
                return fullPath;
            }
        }
        catch (XamlTestException) { }
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
