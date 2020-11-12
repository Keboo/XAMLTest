using System;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using GrpcDotNetNamedPipes;
using XamlTest.Internal;

namespace XamlTest
{
    public static class App
    {
        public static IApp StartRemote<TApp>(
            string? xamlTestPath = null,
            Action<string>? logMessage = null)
        {
            string location = typeof(TApp).Assembly.Location;
            return StartRemote(location, xamlTestPath, logMessage);
        }

        public static IApp StartRemote(
            string? remoteApp = null,
            string ? xamlTestPath = null, 
            Action<string>? logMessage = null)
        {
            xamlTestPath ??= Path.ChangeExtension(Assembly.GetExecutingAssembly().Location, ".exe");
            xamlTestPath = Path.GetFullPath(xamlTestPath);
            if (!File.Exists(xamlTestPath))
            {
                throw new Exception($"Could not find test app '{xamlTestPath}'");
            }

            var startInfo = new ProcessStartInfo(xamlTestPath)
            {
                WorkingDirectory = Path.GetDirectoryName(xamlTestPath) + Path.DirectorySeparatorChar,
                UseShellExecute = true
            };
            if (!string.IsNullOrWhiteSpace(remoteApp))
            {
                startInfo.Arguments = remoteApp;
            }

            if (Process.Start(startInfo) is Process process)
            {
                var channel = new NamedPipeChannel(".", Server.PipePrefix + process.Id, new NamedPipeChannelOptions
                {
                    ConnectionTimeout = 1000
                });
                var client = new Protocol.ProtocolClient(channel);

                return new ManagedApp(process, client, logMessage);
            }
            throw new Exception("Failed t ");
        }
    }
}
