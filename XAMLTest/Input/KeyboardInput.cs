using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Input;

namespace XamlTest.Input
{
    internal static class KeyboardInput
    {
        public static void SendKeys(params Key[] keys)
        {
            var inputs =
               keys.Select(k => (ushort)KeyInterop.VirtualKeyFromKey(k))
                   .SelectMany(k => GetKeyPress(k));

            SendInput(inputs);
        }

        public static void SendKeysForText(string textInput)
        {
            var inputs = textInput.SelectMany(c => GetKeyPress(c));
            SendInput(inputs);
        }

        private static void SendInput(IEnumerable<INPUT> inputs)
        {
            INPUT[]? toSend = inputs.ToArray();
            if (NativeMethods.SendInput((uint)toSend.Length, toSend, Marshal.SizeOf(typeof(INPUT))) != toSend.Length)
            {
                throw new Exception("Some simulated input commands were not sent successfully. The most common reason for this happening are the security features of Windows including User Interface Privacy Isolation (UIPI). Your application can only send commands to applications of the same or lower elevation. Similarly certain commands are restricted to Accessibility/UIAutomation applications.");
            }
        }

        private static IEnumerable<INPUT> GetKeyPress(char character)
        {
            ushort num = character;
            INPUT rv = default;
            rv.Type = 1u;
            rv.Data.Keyboard = new KEYBDINPUT
            {
                KeyCode = 0,
                Scan = num,
                Flags = 4u,
                Time = 0u,
                ExtraInfo = IntPtr.Zero
            };
            INPUT item1 = rv;

            rv = default;
            rv.Type = 1u;
            rv.Data.Keyboard = new KEYBDINPUT
            {
                KeyCode = 0,
                Scan = num,
                Flags = 6u,
                Time = 0u,
                ExtraInfo = IntPtr.Zero
            };

            INPUT item2 = rv;
            if ((num & 0xFF00) == 57344)
            {
                item1.Data.Keyboard.Flags |= 1u;
                item2.Data.Keyboard.Flags |= 1u;
            }

            yield return item1;
            yield return item2;
        }

        private static IEnumerable<INPUT> GetKeyPress(ushort keyCode)
        {
            yield return GetKeyDown(keyCode);
            yield return GetKeyUp(keyCode);
        }

        private static INPUT GetKeyUp(ushort keyCode)
        {
            INPUT rv = default;
            rv.Type = 1u;
            rv.Data.Keyboard = new KEYBDINPUT
            {
                KeyCode = keyCode,
                Scan = 0,
                Flags = IsExtendedKey(keyCode) ? 3u : 2u,
                Time = 0u,
                ExtraInfo = IntPtr.Zero
            };
            return rv;
        }

        private static INPUT GetKeyDown(ushort keyCode)
        {
            INPUT rv = default;
            rv.Type = 1u;
            rv.Data.Keyboard = new KEYBDINPUT
            {
                KeyCode = keyCode,
                Scan = 0,
                Flags = IsExtendedKey(keyCode) ? 1u : 0u,
                Time = 0u,
                ExtraInfo = IntPtr.Zero
            };
            return rv;
        }

        private static bool IsExtendedKey(ushort keyCode)
        {
            switch (keyCode)
            {
                case 18: //MENU:
                case 164: //LMENU:
                case 165: // VirtualKeyCode.RMENU:
                case 17: // VirtualKeyCode.CONTROL:
                case 163: //VirtualKeyCode.RCONTROL:
                case 45: //VirtualKeyCode.INSERT:
                case 46: // VirtualKeyCode.DELETE:
                case 36: // VirtualKeyCode.HOME:
                case 35: // VirtualKeyCode.END:
                case 33: // VirtualKeyCode.PRIOR:
                case 34: // VirtualKeyCode.NEXT:
                case 39: // VirtualKeyCode.RIGHT:
                case 38: // VirtualKeyCode.UP:
                case 37: // VirtualKeyCode.LEFT:
                case 40: // VirtualKeyCode.DOWN:
                case 144: // VirtualKeyCode.NUMLOCK:
                case 3: // VirtualKeyCode.CANCEL:
                case 44: // VirtualKeyCode.SNAPSHOT:
                case 111: // VirtualKeyCode.DIVIDE:
                    return true;
                default:
                    return false;
            }
        }

    }
}
