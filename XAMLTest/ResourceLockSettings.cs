using System.Diagnostics;

namespace XamlTest;

public static class ResourceLockSettings
{
    private static TimeSpan _defaultTimeout = Debugger.IsAttached
        ? TimeSpan.FromMinutes(10)
        : TimeSpan.FromSeconds(60);

    public static TimeSpan DefaultTimeout
    {
        get => _defaultTimeout;
        set
        {
            if (value < TimeSpan.Zero && value != Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(value), value, "Default timeout must be non-negative or Timeout.InfiniteTimeSpan.");
            }

            _defaultTimeout = value;
        }
    }
}
