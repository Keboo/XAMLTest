using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
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
                CreateProcessFlags.None, IntPtr.Zero, 
                Path.GetDirectoryName(path) + Path.DirectorySeparatorChar, 
                ref startupInfo, out PROCESS_INFORMATION processInfo);

            if (!rv) throw new InvalidOperationException();

            var process = Process.GetProcessById(processInfo.dwProcessId);
            var channel = new NamedPipeChannel(".", Server.PipePrefix + process.Id);
            var client = new Protocol.ProtocolClient(channel);

            return new ManagedApp(process, client, logMessage);
        }
    }
}
