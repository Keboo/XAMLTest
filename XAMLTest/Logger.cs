namespace XamlTest;

internal static class Logger
{
    private static List<string> LogMessages { get; } = new();
    private static List<StreamWriter> Writers { get; } = new();

    static Logger()
    {
        AddLogOutput(File.Open($"XAMLTest.{Process.GetCurrentProcess().Id}.log", FileMode.Create, FileAccess.Write, FileShare.Read));
    }

    public static void AddLogOutput(Stream stream)
    {
        StreamWriter writer = new(stream) { AutoFlush = true };
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
        message = $"{DateTime.Now} - {message}";
        lock (LogMessages)
        {
            LogMessages.Add(message);
            foreach(var writer in Writers)
            {
                writer.WriteLine(message);
                writer.Flush();
            }
        }
    }
}
