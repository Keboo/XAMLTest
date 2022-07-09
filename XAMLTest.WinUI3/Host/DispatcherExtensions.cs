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
            tcs.SetResult(action());
        });
        if (success)
        {
            return tcs.Task;
        }
        return Task.FromResult(default(T));
    }
}
