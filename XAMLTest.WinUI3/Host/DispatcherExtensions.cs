using Microsoft.UI.Dispatching;

namespace XamlTest.Host;

internal static class DispatcherExtensions
{
    public static Task<bool> TryInvokeAsync(this DispatcherQueue dispatcher, Action action)
        => TryInvokeAsync(dispatcher, () =>
        {
            action();
            return true;
        });

    public static Task<T?> TryInvokeAsync<T>(this DispatcherQueue dispatcher, Func<T?> action)
    {
        TaskCompletionSource<T?> tcs = new();
        bool success = dispatcher.TryEnqueue(() =>
        {
            try
            {
                T? result = action();
                tcs.SetResult(result);
            }
            catch (Exception e)
            {
                tcs.SetException(e);
            }
        });
        if (success)
        {
            return tcs.Task;
        }
        return Task.FromResult(default(T));
    }
}
