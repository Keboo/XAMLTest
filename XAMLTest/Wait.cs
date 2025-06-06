namespace XamlTest;

[DebuggerStepThrough]
public static class Wait
{
    public static async Task For(Func<Task<bool>> action, Retry? retry = null, string? message = null)
    {
        if (action is null)
        {
            throw new ArgumentNullException(nameof(action));
        }

        retry ??= Retry.Default;

        int delay = (int)(retry.Timeout.TotalMilliseconds / 10);
        if (delay < 15)
        {
            delay = 0;
        }

        int numAttempts = 0;
        var sw = Stopwatch.StartNew();
        Exception? thrownException = null;
        do
        {
            numAttempts++;
            try
            {
                if (await action())
                {
                    //Success
                    return;
                }
            }
            catch (Exception ex)
            {
                thrownException = ex;
            }
            if (delay > 0)
            {
                await Task.Delay(delay);
            }
        }
        while (ShouldRetry());
        var prefix = message == null ? string.Empty : $"{message}. ";
        throw new TimeoutException($"{prefix}Timeout of '{retry}' exceeded", thrownException);

        bool ShouldRetry() =>
            sw.Elapsed <= retry.Timeout ||
            numAttempts < retry.MinAttempts;
    }

    public static async Task For(Func<Task> action, Retry? retry = null, string? message = null)
    {
        await For(async () =>
        {
            await action();
            return true;
        }, retry, message);
    }

    public static async Task<T> For<T>(Func<Task<T>> action, Retry? retry = null, string? message = null)
        where T : class
    {
        T? rv = default;
        await For(async () =>
        {
            rv = await action();
            return true;
        }, retry, message);

        return rv ?? throw new XamlTestException("Return value is null");
    }
}
