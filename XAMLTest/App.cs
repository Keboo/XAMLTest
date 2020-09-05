using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using GrpcDotNetNamedPipes;
using XamlTest.Internal;
using static PInvoke.Kernel32;

namespace XamlTest
{
    public static class App
    {
        public static IApp StartRemote(string? path = null, Action<string>? logMessage = null)
        {
            path ??= Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe");
            path = Path.GetFullPath(path);
            if (!File.Exists(path))
            {
                throw new Exception($"Could not find test app '{path}'");
            }

            //var startInfo = new ProcessStartInfo(path)
            //{
            //    WorkingDirectory = Path.GetDirectoryName(path) + Path.DirectorySeparatorChar,
            //    UseShellExecute = true
            //};
            //Process process = Process.Start(startInfo);
            var startupInfo = new STARTUPINFO();

            bool rv = CreateProcess(path, "", IntPtr.Zero, IntPtr.Zero, false, 
                CreateProcessFlags.DEBUG_ONLY_THIS_PROCESS, IntPtr.Zero, 
                Path.GetDirectoryName(path) + Path.DirectorySeparatorChar, 
                ref startupInfo, out PROCESS_INFORMATION processInfo);

            if (!rv) throw new InvalidOperationException();

            var process = Process.GetProcessById(processInfo.dwProcessId);
            Task.Run(() =>
            {
                while(!process.HasExited)
                {
                    var debugEvent = new DEBUG_EVENT
                    {
                        dwProcessId = processInfo.dwProcessId,
                        dwThreadId = processInfo.dwThreadId
                    };

                    if (WaitForDebugEvent(ref debugEvent, 100))
                    {
                        ContinueDebugEvent(debugEvent.dwProcessId, debugEvent.dwThreadId, ContinueStatus.DBG_EXCEPTION_NOT_HANDLED);
                    }
                    process.Refresh();
                }
            });

            var channel = new NamedPipeChannel(".", Server.PipePrefix + process.Id);
            var client = new Protocol.ProtocolClient(channel);

            return new ManagedApp(process, client, logMessage);
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        static extern bool ContinueDebugEvent(int dwProcessId, int dwThreadId, ContinueStatus dwContinueStatus);

        [DllImport("kernel32.dll", EntryPoint = "WaitForDebugEvent")]
        [return: MarshalAs(UnmanagedType.Bool)]
        private static extern bool WaitForDebugEvent(ref DEBUG_EVENT lpDebugEvent, uint dwMilliseconds);
    }

    public enum ContinueStatus : uint
    {
        DBG_CONTINUE = 0x00010002,
        DBG_EXCEPTION_NOT_HANDLED = 0x80010001,
        DBG_REPLY_LATER = 0x40010001
    }

    public enum DebugEventType : uint
    {
        RIP_EVENT = 9,
        OUTPUT_DEBUG_STRING_EVENT = 8,
        UNLOAD_DLL_DEBUG_EVENT = 7,
        LOAD_DLL_DEBUG_EVENT = 6,
        EXIT_PROCESS_DEBUG_EVENT = 5,
        EXIT_THREAD_DEBUG_EVENT = 4,
        CREATE_PROCESS_DEBUG_EVENT = 3,
        CREATE_THREAD_DEBUG_EVENT = 2,
        EXCEPTION_DEBUG_EVENT = 1,
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXCEPTION_DEBUG_INFO
    {
        public EXCEPTION_RECORD ExceptionRecord;
        public uint dwFirstChance;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXCEPTION_RECORD
    {
        public uint ExceptionCode;
        public uint ExceptionFlags;
        public IntPtr ExceptionRecord;
        public IntPtr ExceptionAddress;
        public uint NumberParameters;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 15, ArraySubType = UnmanagedType.U4)] public uint[] ExceptionInformation;
    }

    public delegate uint PTHREAD_START_ROUTINE(IntPtr lpThreadParameter);

    [StructLayout(LayoutKind.Sequential)]
    public struct CREATE_THREAD_DEBUG_INFO
    {
        public IntPtr hThread;
        public IntPtr lpThreadLocalBase;
        public PTHREAD_START_ROUTINE lpStartAddress;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct CREATE_PROCESS_DEBUG_INFO
    {
        public IntPtr hFile;
        public IntPtr hProcess;
        public IntPtr hThread;
        public IntPtr lpBaseOfImage;
        public uint dwDebugInfoFileOffset;
        public uint nDebugInfoSize;
        public IntPtr lpThreadLocalBase;
        public PTHREAD_START_ROUTINE lpStartAddress;
        public IntPtr lpImageName;
        public ushort fUnicode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXIT_THREAD_DEBUG_INFO
    {
        public uint dwExitCode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct EXIT_PROCESS_DEBUG_INFO
    {
        public uint dwExitCode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct LOAD_DLL_DEBUG_INFO
    {
        public IntPtr hFile;
        public IntPtr lpBaseOfDll;
        public uint dwDebugInfoFileOffset;
        public uint nDebugInfoSize;
        public IntPtr lpImageName;
        public ushort fUnicode;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct UNLOAD_DLL_DEBUG_INFO
    {
        public IntPtr lpBaseOfDll;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct OUTPUT_DEBUG_STRING_INFO
    {
        [MarshalAs(UnmanagedType.LPStr)] public string lpDebugStringData;
        public ushort fUnicode;
        public ushort nDebugStringLength;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct RIP_INFO
    {
        public uint dwError;
        public uint dwType;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct DEBUG_EVENT
    {
        public DebugEventType dwDebugEventCode;
        public int dwProcessId;
        public int dwThreadId;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 86, ArraySubType = UnmanagedType.U1)]
        byte[] debugInfo;

        public EXCEPTION_DEBUG_INFO Exception
        {
            get { return GetDebugInfo<EXCEPTION_DEBUG_INFO>(); }
        }

        public CREATE_THREAD_DEBUG_INFO CreateThread
        {
            get { return GetDebugInfo<CREATE_THREAD_DEBUG_INFO>(); }
        }

        public CREATE_PROCESS_DEBUG_INFO CreateProcessInfo
        {
            get { return GetDebugInfo<CREATE_PROCESS_DEBUG_INFO>(); }
        }

        public EXIT_THREAD_DEBUG_INFO ExitThread
        {
            get { return GetDebugInfo<EXIT_THREAD_DEBUG_INFO>(); }
        }

        public EXIT_PROCESS_DEBUG_INFO ExitProcess
        {
            get { return GetDebugInfo<EXIT_PROCESS_DEBUG_INFO>(); }
        }

        public LOAD_DLL_DEBUG_INFO LoadDll
        {
            get { return GetDebugInfo<LOAD_DLL_DEBUG_INFO>(); }
        }

        public UNLOAD_DLL_DEBUG_INFO UnloadDll
        {
            get { return GetDebugInfo<UNLOAD_DLL_DEBUG_INFO>(); }
        }

        public OUTPUT_DEBUG_STRING_INFO DebugString
        {
            get { return GetDebugInfo<OUTPUT_DEBUG_STRING_INFO>(); }
        }

        public RIP_INFO RipInfo
        {
            get { return GetDebugInfo<RIP_INFO>(); }
        }

        private T GetDebugInfo<T>() where T : struct
        {
            var structSize = Marshal.SizeOf(typeof(T));
            var pointer = Marshal.AllocHGlobal(structSize);
            Marshal.Copy(debugInfo, 0, pointer, structSize);

            var result = Marshal.PtrToStructure(pointer, typeof(T));
            Marshal.FreeHGlobal(pointer);
            return (T)result;
        }
    }
}
