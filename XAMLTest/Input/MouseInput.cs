using System.Windows;
using Windows.Win32;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace XamlTest.Input;

internal static class MouseInput
{
    public static void LeftClick()
    {
        LeftDown();
        LeftUp();
    }

    public static void RightClick()
    {
        RightDown();
        RightUp();
    }

    public static void MiddleClick()
    {
        MiddleDown();
        MiddleUp();
    }

    public static void LeftDown()
        => MouseEvent(MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTDOWN);

    public static void LeftUp()
        => MouseEvent(MOUSE_EVENT_FLAGS.MOUSEEVENTF_LEFTUP);

    public static void RightDown()
        => MouseEvent(MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTDOWN);

    public static void RightUp()
        => MouseEvent(MOUSE_EVENT_FLAGS.MOUSEEVENTF_RIGHTUP);

    public static void MiddleDown()
        => MouseEvent(MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEDOWN);

    public static void MiddleUp()
        => MouseEvent(MOUSE_EVENT_FLAGS.MOUSEEVENTF_MIDDLEUP);

    public static void MoveCursor(Point screenLocation)
        => PInvoke.SetCursorPos((int)screenLocation.X, (int)screenLocation.Y);

    public static Point GetCursorPosition()
    {
        PInvoke.GetCursorPos(out var pos);
        return new Point(pos.X, pos.Y);
    }

    private static void MouseEvent(MOUSE_EVENT_FLAGS flags)
    {
        PInvoke.mouse_event(flags, 0, 0, 0, 0);
    }

}
