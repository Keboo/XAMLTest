using System.Collections.Generic;
using System.IO;

namespace XamlTest;

internal static class Logger
{
    private static List<string> LogMessages { get; } = new();
    private static List<StreamWriter> Writers { get; } = new();

    public static void AddLogOutput(Stream stream)
    {
        StreamWriter writer = new(stream);
        lock(LogMessages)
        {
            Writers.Add(writer);
        }
    }

    public static void CloseLogger()
    {
        lock (LogMessages)
        {
            foreach(var writer in Writers)
            {
                writer.Flush();
                writer.Dispose();
            }
            Writers.Clear();
        }
    }


    public static IReadOnlyList<string> GetMessages()
    {
        lock (LogMessages)
        {
            return LogMessages.AsReadOnly();
        }
    }

    public static void Log(string message)
    {
        lock (LogMessages)
        {
            LogMessages.Add(message);
            foreach(var writer in Writers)
            {
                writer.WriteLine(message);
            }
        }
    }
}
