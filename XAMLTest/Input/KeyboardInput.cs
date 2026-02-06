using System.Windows.Input;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Input.KeyboardAndMouse;

namespace XamlTest.Input;

internal static class KeyboardInput
{
    private const uint WM_CHAR = 0x0102;
    private const uint WM_KEYDOWN = 0x0100;
    private const uint WM_KEYUP = 0x0101;

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
        HWND hwnd = new(windowHandle);
        foreach (WindowMessage message in messages)
        {
            PInvoke.SendMessage(hwnd, message.Message, new WPARAM((nuint)message.WParam), new LPARAM(message.LParam));
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
            PInvoke.SendInput([input.Input], sizeOfInputStruct);
            //https://source.dot.net/#System.Windows.Forms/System/Windows/Forms/SendKeys.cs,720
            Thread.Sleep(1);
        }
    }

    private static IEnumerable<WindowMessage> GetKeyPress(char character)
    {
        nint wParam = character;
        nint lParam = 0x0000_0000;
        yield return new WindowMessage(WM_CHAR, wParam, lParam);
    }

    private static IEnumerable<WindowMessage> GetKeyPress(Key key)
    {
        nint wParam = KeyInterop.VirtualKeyFromKey(key);
        nint lParam = 0x0000_0000;
        yield return new WindowMessage(WM_KEYDOWN, wParam, lParam);
        yield return new WindowMessage(WM_KEYUP, wParam, lParam);
    }

    private static IEnumerable<WindowInput> GetKeyPress(ModifierKeys modifiers)
    {
        if (modifiers == ModifierKeys.None)
        {
            // Special case to remove any modifiers previously set, so we send KEYUP for all modifiers
            yield return new WindowInput(CreateInput(VIRTUAL_KEY.VK_MENU, true));
            yield return new WindowInput(CreateInput(VIRTUAL_KEY.VK_CONTROL, true));
            yield return new WindowInput(CreateInput(VIRTUAL_KEY.VK_SHIFT, true));
            yield return new WindowInput(CreateInput(VIRTUAL_KEY.VK_LWIN, true));
        }
        else
        {
            yield return new WindowInput(CreateInput(VIRTUAL_KEY.VK_MENU, !modifiers.HasFlag(ModifierKeys.Alt)));
            yield return new WindowInput(CreateInput(VIRTUAL_KEY.VK_CONTROL, !modifiers.HasFlag(ModifierKeys.Control)));
            yield return new WindowInput(CreateInput(VIRTUAL_KEY.VK_SHIFT, !modifiers.HasFlag(ModifierKeys.Shift)));
            yield return new WindowInput(CreateInput(VIRTUAL_KEY.VK_LWIN, !modifiers.HasFlag(ModifierKeys.Windows)));
        }
    }

    private static INPUT CreateInput(VIRTUAL_KEY modifierKey, bool keyUp)
    {
        INPUT input = new()
        {
            type = INPUT_TYPE.INPUT_KEYBOARD
        };
        input.Anonymous.ki.wVk = modifierKey;
        if (keyUp)
        {
            input.Anonymous.ki.dwFlags = KEYBD_EVENT_FLAGS.KEYEVENTF_KEYUP;
        }
        return input;
    }

    private class WindowMessage
    {
        public WindowMessage(uint message, nint wParam, nint lParam)
        {
            Message = message;
            WParam = wParam;
            LParam = lParam;
        }

        public uint Message { get; }
        public nint WParam { get; }
        public nint LParam { get; }
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
