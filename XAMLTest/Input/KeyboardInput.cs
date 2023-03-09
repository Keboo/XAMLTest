using PInvoke;
using System.Windows.Input;
using static PInvoke.User32;

namespace XamlTest.Input;

internal static class KeyboardInput
{
    public static void SendModifiers(IntPtr windowHandle, params ModifierKeys[] modifiers)
    {
        IEnumerable<WindowInput> inputs = modifiers.SelectMany(GetKeyPress);
        SendInput(windowHandle, inputs);
    }

    public static void SendKeys(IntPtr windowHandle, params Key[] keys)
    {
        IEnumerable<WindowMessage> inputs = keys.SelectMany(GetKeyPress);
        SendInput(windowHandle, inputs);
    }

    public static void SendKeysForText(IntPtr windowHandle, string textInput)
    {
        IEnumerable<WindowMessage> inputs = textInput.SelectMany(GetKeyPress);
        SendInput(windowHandle, inputs);
    }

    private static void SendInput(IntPtr windowHandle, IEnumerable<WindowMessage> messages)
    {
        foreach (WindowMessage message in messages)
        {
            User32.SendMessage(windowHandle, message.Message, message.WParam, message.LParam);
        }
    }

    private static void SendInput(IntPtr windowHandle, IEnumerable<WindowInput> inputs)
    {
        int sizeOfInputStruct;
        unsafe
        {
            // NOTE: There is a potential x86/x64 size issue here
            sizeOfInputStruct = sizeof(INPUT);
        }

        foreach (WindowInput input in inputs)
        {
            User32.SendInput(1, new[] { input.Input }, sizeOfInputStruct);
            //https://source.dot.net/#System.Windows.Forms/System/Windows/Forms/SendKeys.cs,720
            Thread.Sleep(1);
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

    private static IEnumerable<WindowInput> GetKeyPress(ModifierKeys modifiers)
    {
        if (modifiers == ModifierKeys.None)
        {
            // Special case to remove any modifiers previously set, so we send KEYUP for all modifiers
            yield return new WindowInput(CreateInput(VirtualKey.VK_MENU, true));
            yield return new WindowInput(CreateInput(VirtualKey.VK_CONTROL, true));
            yield return new WindowInput(CreateInput(VirtualKey.VK_SHIFT, true));
            yield return new WindowInput(CreateInput(VirtualKey.VK_LWIN, true));
        }
        else
        {
            yield return new WindowInput(CreateInput(VirtualKey.VK_MENU, !modifiers.HasFlag(ModifierKeys.Alt)));
            yield return new WindowInput(CreateInput(VirtualKey.VK_CONTROL, !modifiers.HasFlag(ModifierKeys.Control)));
            yield return new WindowInput(CreateInput(VirtualKey.VK_SHIFT, !modifiers.HasFlag(ModifierKeys.Shift)));
            yield return new WindowInput(CreateInput(VirtualKey.VK_LWIN, !modifiers.HasFlag(ModifierKeys.Windows)));
        }
    }

    private static INPUT CreateInput(VirtualKey modifierKey, bool keyUp)
    {
        INPUT input = new()
        {
            type = User32.InputType.INPUT_KEYBOARD
        };
        input.Inputs.ki.wVk = modifierKey;
        if (keyUp)
        {
            input.Inputs.ki.dwFlags = KEYEVENTF.KEYEVENTF_KEYUP;
        }
        return input;
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

    private class WindowInput
    {
        public WindowInput(INPUT input)
        {
            Input = input;
        }

        public INPUT Input { get; }
    }
}
