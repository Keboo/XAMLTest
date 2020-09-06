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
            var inputs =
               keys.Select(k => (ushort)KeyInterop.VirtualKeyFromKey(k))
                   .SelectMany(k => GetKeyPress(k));

            SendInput(windowHandle, inputs);
        }

        public static void SendKeysForText(IntPtr windowHandle, string textInput)
        {
            var inputs = textInput.SelectMany(c => GetKeyPress(c));
            SendInput(windowHandle, inputs);
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

        private static void SendInput(IntPtr windowHandle, IEnumerable<WindowMessage> messages)
        {
            foreach (WindowMessage message in messages)
            {
                User32.SendMessage(windowHandle, message.Message, message.WParam, message.LParam);
            }
        }

        private static IEnumerable<WindowMessage> GetKeyPress(char character)
        {
            var wParam = new IntPtr(character);
            var lParam = new IntPtr(0x0000_0000);
            yield return new WindowMessage(User32.WindowMessage.WM_CHAR, wParam, lParam);
        }

        private static IEnumerable<WindowMessage> GetKeyPress(ushort keyCode)
        {
            var wParam = new IntPtr(keyCode);
            var lParam = new IntPtr(0x0000_0000);
            yield return new WindowMessage(User32.WindowMessage.WM_KEYDOWN, wParam, lParam);
            yield return new WindowMessage(User32.WindowMessage.WM_KEYUP, wParam, lParam);

        }

    }
}
