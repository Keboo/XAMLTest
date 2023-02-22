using PInvoke;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using static PInvoke.User32;

namespace XamlTest.Input;

internal static class KeyboardInput
{
    public static void SendModifiers(IntPtr windowHandle, params ModifierKeys[] modifiers)
    {
        IEnumerable<WindowMessage> inputs = modifiers.SelectMany(m => GetKeyPress(m));
        SendInput(windowHandle, inputs);
    }

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

    private static IEnumerable<WindowMessage> GetKeyPress(ModifierKeys modifiers)
    {
        // TODO: The messages returned from this method currently do not do what we expect them to!

        IntPtr lParam = new(0x0000_0000);
        if (modifiers == ModifierKeys.None)
        {
            // Special case to remove any modifiers previously set, so we send KEYUP for all modifiers
            yield return new WindowMessage(User32.WindowMessage.WM_SYSKEYUP, new IntPtr((int)User32.VirtualKey.VK_MENU), lParam);  // VK_MENU is an alias for the ALT key
            yield return new WindowMessage(User32.WindowMessage.WM_SYSKEYUP, new IntPtr((int)User32.VirtualKey.VK_CONTROL), lParam);
            yield return new WindowMessage(User32.WindowMessage.WM_SYSKEYUP, new IntPtr((int)User32.VirtualKey.VK_SHIFT), lParam);
            yield return new WindowMessage(User32.WindowMessage.WM_SYSKEYUP, new IntPtr((int)User32.VirtualKey.VK_LWIN), lParam);
        }
        else
        {
            if (modifiers.HasFlag(ModifierKeys.Alt))
            {
                yield return new WindowMessage(User32.WindowMessage.WM_SYSKEYDOWN, new IntPtr((int)User32.VirtualKey.VK_MENU), lParam);  // VK_MENU is an alias for the ALT key
            }
            if (modifiers.HasFlag(ModifierKeys.Control)) 
            {
                yield return new WindowMessage(User32.WindowMessage.WM_SYSKEYDOWN, new IntPtr((int)User32.VirtualKey.VK_CONTROL), lParam);
            }
            if (modifiers.HasFlag(ModifierKeys.Shift))
            {
                yield return new WindowMessage(User32.WindowMessage.WM_SYSKEYDOWN, new IntPtr((int)User32.VirtualKey.VK_SHIFT), lParam);
            }
            if (modifiers.HasFlag(ModifierKeys.Windows)) 
            {
                yield return new WindowMessage(User32.WindowMessage.WM_SYSKEYDOWN, new IntPtr((int)User32.VirtualKey.VK_LWIN), lParam);
            }
        }
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
