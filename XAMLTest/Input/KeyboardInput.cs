using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Input;

namespace XamlTest.Input
{
    internal static class KeyboardInput
    {
        public static void SendKeys(IntPtr windowHandle, params Key[] keys)
        {
            IEnumerable<WindowMessage> inputs = keys.SelectMany(k => GetKeyPress(k));
            SendInput(windowHandle, inputs);
        }

        public static void SendKeysForText(IntPtr windowHandle, string textInput)
        {
            IEnumerable<WindowMessage> inputs = textInput.SelectMany(c => GetKeyPress(c));
            SendInput(windowHandle, inputs);
        }

        private static void SendInput(IntPtr windowHandle, IEnumerable<WindowMessage> messages)
        {
            foreach (WindowMessage message in messages)
            {
                User32.SendMessage(windowHandle, message.Message, message.WParam, message.LParam);
            }
        }

        private static IEnumerable<WindowMessage> GetKeyPress(char character)
        {
            IntPtr wParam = new(character);
            IntPtr lParam = new(0x0000_0000);
            yield return new WindowMessage(User32.WindowMessage.WM_CHAR, wParam, lParam);
        }

        private static IEnumerable<WindowMessage> GetKeyPress(Key key)
        {
            IntPtr wParam = new(KeyInterop.VirtualKeyFromKey(key));
            IntPtr lParam = new(0x0000_0000);
            yield return new WindowMessage(User32.WindowMessage.WM_KEYDOWN, wParam, lParam);
            yield return new WindowMessage(User32.WindowMessage.WM_KEYUP, wParam, lParam);
        }

        private class WindowMessage
        {
            public WindowMessage(User32.WindowMessage message, IntPtr wParam, IntPtr lParam)
            {
                Message = message;
                WParam = wParam;
                LParam = lParam;
            }

            public User32.WindowMessage Message { get; }
            public IntPtr WParam { get; }
            public IntPtr LParam { get; }
        }
    }
}
