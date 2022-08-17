using System;
using System.Collections.Generic;
using System.Windows;

namespace XamlTest;

internal static class Logger
{
    private static readonly DependencyProperty LogProperty = DependencyProperty.RegisterAttached(
       "Log",
       typeof(List<string>),
       typeof(Logger),
       new PropertyMetadata(null));

    public static IReadOnlyList<string> GetLogMessages(this DependencyObject source)
    {
        if (source is null) return Array.Empty<string>();
        lock (LogProperty)
        {
            List<string> logs = (List<string>)source.GetValue(LogProperty);
            return logs?.AsReadOnly() ?? (IReadOnlyList<string>)Array.Empty<string>();
        }
    }

    public static void LogMessage(this DependencyObject source, string message)
    {
        if (source is null) return;
        lock (LogProperty)
        {
            List<string> logs = (List<string>)source.GetValue(LogProperty);
            if (logs is null)
            {
                logs = new List<string>();
                source.SetValue(LogProperty, logs);
            }
            logs.Add(message);
        }
    }
}
