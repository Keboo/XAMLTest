using System;
using System.Runtime.InteropServices;

namespace XamlTest.Input
{
    internal static class NativeMethods
    {
        [DllImport("user32.dll", SetLastError = true)]
        public static extern short GetAsyncKeyState(ushort virtualKeyCode);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern short GetKeyState(ushort virtualKeyCode);

        [DllImport("user32.dll", SetLastError = true)]
        public static extern uint SendInput(uint numberOfInputs, INPUT[] inputs, int sizeOfInputStructure);

        [DllImport("user32.dll")]
        public static extern IntPtr GetMessageExtraInfo();
    }

    internal struct INPUT
    {
        public uint Type;

        public MOUSEKEYBDHARDWAREINPUT Data;
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct MOUSEKEYBDHARDWAREINPUT
    {
        [FieldOffset(0)]
        public MOUSEINPUT Mouse;

        [FieldOffset(0)]
        public KEYBDINPUT Keyboard;

        [FieldOffset(0)]
        public HARDWAREINPUT Hardware;
    }

    internal struct MOUSEINPUT
    {
        public int X;

        public int Y;

        public uint MouseData;

        public uint Flags;

        public uint Time;

        public IntPtr ExtraInfo;
    }

    internal struct KEYBDINPUT
    {
        public ushort KeyCode;

        public ushort Scan;

        public uint Flags;

        public uint Time;

        public IntPtr ExtraInfo;
    }

    internal struct HARDWAREINPUT
    {
        public uint Msg;

        public ushort ParamL;

        public ushort ParamH;
    }
}
