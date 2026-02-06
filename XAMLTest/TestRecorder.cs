using System.Runtime.CompilerServices;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace XamlTest;

public sealed class TestRecorder : IAsyncDisposable
{
    private record class InputStates
    {
        public bool IsLeftButtonDown { get; init; }
        public bool IsRightButtonDown { get; init; }
        public bool IsMiddleButtonDown { get; init; }

        public bool IsShiftDown { get; init; }
        public bool IsControlDown { get; init; }
        public bool IsAltDown { get; init; }

        public static InputStates Empty { get; } = new();

        public static InputStates GetCurrentState()
        {
            return new InputStates
            {
                IsLeftButtonDown = GetCurrentState(VIRTUAL_KEY.VK_LBUTTON),
                IsRightButtonDown = GetCurrentState(VIRTUAL_KEY.VK_RBUTTON),
                IsMiddleButtonDown = GetCurrentState(VIRTUAL_KEY.VK_MBUTTON),
                IsShiftDown = GetCurrentState(VIRTUAL_KEY.VK_SHIFT),
                IsControlDown = GetCurrentState(VIRTUAL_KEY.VK_CONTROL),
                IsAltDown = GetCurrentState(VIRTUAL_KEY.VK_MENU)
            };
        }

        private static bool GetCurrentState(VIRTUAL_KEY key)
        {
            var ctrl = PInvoke.GetAsyncKeyState((int)key);
            return ((ushort)ctrl >> 15) == 1;
        }
    }

    public IApp App { get; }
    public string BaseFileName { get; }

    private InputStates Inputs { get; } = InputStates.GetCurrentState();

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
        if (Inputs != InputStates.Empty)
        {
            App.LogMessage($"WARNING: Test started with initial input states: {Inputs}");
        }
    }

    /// <summary>
    /// Calling this method indicates that the test completed successfully and no additional recording is needed.
    /// </summary>
    public void Success(bool skipInputStateCheck = false)
    {
        if (!skipInputStateCheck) 
        {
            var endingState = InputStates.GetCurrentState();
            if (endingState != Inputs)
            {
                StringBuilder sb = new();
                sb.AppendLine("Input states were not reset at the end of the test:");
                if (endingState.IsLeftButtonDown != Inputs.IsLeftButtonDown)
                    sb.AppendLine($"  Left button down: {endingState.IsLeftButtonDown} (was {Inputs.IsLeftButtonDown})");
                if (endingState.IsRightButtonDown != Inputs.IsRightButtonDown)
                    sb.AppendLine($"  Right button down: {endingState.IsRightButtonDown} (was {Inputs.IsRightButtonDown})");
                if (endingState.IsMiddleButtonDown != Inputs.IsMiddleButtonDown)
                    sb.AppendLine($"  Middle button down: {endingState.IsMiddleButtonDown} (was {Inputs.IsMiddleButtonDown})");
                if (endingState.IsShiftDown != Inputs.IsShiftDown)
                    sb.AppendLine($"  Shift key down: {endingState.IsShiftDown} (was {Inputs.IsShiftDown})");
                if (endingState.IsControlDown != Inputs.IsControlDown)
                    sb.AppendLine($"  Control key down: {endingState.IsControlDown} (was {Inputs.IsControlDown})");
                if (endingState.IsAltDown != Inputs.IsAltDown)
                    sb.AppendLine($"  Alt key down: {endingState.IsAltDown} (was {Inputs.IsAltDown})");
                throw new XamlTestException(sb.ToString());
            }
            App.LogMessage("Input states matched starting state.");
        }
        IsSuccess = true;
    }

    /// <summary>
    /// Enumerate all screenshots
    /// </summary>
    /// <returns></returns>
    public IEnumerable<string> EnumerateScreenshots()
    {
        if (!System.IO.Directory.Exists(Directory))
        {
            return [];
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
