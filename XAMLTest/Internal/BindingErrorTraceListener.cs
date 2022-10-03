namespace XamlTest.Internal;

internal sealed class BindingErrorTraceListener : TraceListener
{
    public override bool IsThreadSafe => true;

    private readonly object _lock = new();
    private List<string> Errors { get; } = new();
    private StringBuilder Current { get; } = new();

    public IReadOnlyList<string> GetErrors(bool clear)
    {
        lock (_lock)
        {
            var rv = Errors.ToList();
            if (clear)
            {
                Errors.Clear();
            }
            return rv;
        }
    }

    public BindingErrorTraceListener()
    {
        Name = "XAMLTest.BindingErrorsListener";
    }

    public override void Write(string? message)
    {
        lock(_lock)
        {
            Current.Append(message);
        }
    }
    
    public override void WriteLine(string? message)
    {
        lock (_lock)
        {
            Current.Append(message);
            Errors.Add(Current.ToString());
            Current.Clear();
        }
    }
}
