using System;
using System.Diagnostics;
using System.Windows;
using XamlTest.Internal;

namespace XamlTest;

public static class Server
{
    internal const string PipePrefix = nameof(DependencyObjectTracker) + "ComminicationPipe";

    internal static IDisposable Start(Application? app = null)
    {
        var process = Process.GetCurrentProcess();
        Service service = new(process.Id.ToString(), app ?? Application.Current);
        return service;
    }
}
