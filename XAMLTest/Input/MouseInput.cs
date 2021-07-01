﻿using System.Windows;
using static PInvoke.User32;

namespace XamlTest.Input
{
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
            => MouseEvent(mouse_eventFlags.MOUSEEVENTF_LEFTDOWN);

        public static void LeftUp()
            => MouseEvent(mouse_eventFlags.MOUSEEVENTF_LEFTUP);

        public static void RightDown()
            => MouseEvent(mouse_eventFlags.MOUSEEVENTF_RIGHTDOWN);

        public static void RightUp()
            => MouseEvent(mouse_eventFlags.MOUSEEVENTF_RIGHTUP);

        public static void MiddleDown()
            => MouseEvent(mouse_eventFlags.MOUSEEVENTF_MIDDLEDOWN);

        public static void MiddleUp()
            => MouseEvent(mouse_eventFlags.MOUSEEVENTF_MIDDLEUP);

        public static void MoveCursor(Point screenLocation)
            => SetCursorPos((int)screenLocation.X, (int)screenLocation.Y);

        private static unsafe void MouseEvent(mouse_eventFlags flags)
        {
            mouse_event(flags, 0, 0, 0, null);
        }
    }
}
