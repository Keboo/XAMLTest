using System;
using System.Diagnostics;
using GrpcDotNetNamedPipes;
using XamlTest.Host;

namespace XamlTest;

public static class Client
{
    public static IApp ConnectToApp()
        => ConnectToApp(Process.GetCurrentProcess());

    public static IApp ConnectToApp(int processId)
        => ConnectToApp(Process.GetProcessById(processId));

    public static IApp ConnectToApp(Process process, Action<string>? logMessage = null)
    {
        if (process is null)
        {
            throw new ArgumentNullException(nameof(process));
        }
        NamedPipeChannel channel = new(".", Server.PipePrefix + process.Id);
        Protocol.ProtocolClient client = new(channel);

        return new Internal.App(client, logMessage);
    }
}
