
using System.Windows;
using static PInvoke.User32;

namespace XamlTest.Input
{
    internal static class MouseInput
    {
        public static unsafe void LeftClick()
            => mouse_event(mouse_eventFlags.MOUSEEVENTF_LEFTDOWN | mouse_eventFlags.MOUSEEVENTF_LEFTUP, 0, 0, 0, null);

        public static void MoveCursor(Point screenLocation)
            => SetCursorPos((int)screenLocation.X, (int)screenLocation.Y);
    }
}
