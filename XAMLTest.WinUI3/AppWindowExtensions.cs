using Microsoft.UI.Windowing;

namespace XamlTest;

public static class AppWindowExtensions
{
    public static AppWindow GetAppWindow(this Window window)
    {
        IntPtr windowHandle = WinRT.Interop.WindowNative.GetWindowHandle(window);
        return GetAppWindowFromWindowHandle(windowHandle);
    }

    private static AppWindow GetAppWindowFromWindowHandle(IntPtr windowHandle)
    {
        WindowId windowId = Win32Interop.GetWindowIdFromWindow(windowHandle);
        return AppWindow.GetFromWindowId(windowId);
    }
}
