using System.Diagnostics;

namespace XamlTest;

public static partial class AppMixins
{
    public static ValueTask<IAsyncDisposable> AcquireResourceLocksAsync(
        this IApp app,
        ResourceLocks locks,
        TimeSpan? timeout = null,
        CancellationToken cancellationToken = default)
    {
        if (app is null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return ResourceLockLease.Acquire(locks, timeout ?? ResourceLockSettings.DefaultTimeout, cancellationToken);
    }

    private sealed class ResourceLockLease : IAsyncDisposable
    {
        private static IAsyncDisposable Empty { get; } = new ResourceLockLease();

        private static IReadOnlyList<(ResourceLocks Lock, string Name)> OrderedLocks { get; } =
        [
            (ResourceLocks.Keyboard, @"Local\XamlTest.ResourceLocks.Keyboard"),
            (ResourceLocks.Mouse, @"Local\XamlTest.ResourceLocks.Mouse"),
            (ResourceLocks.Focus, @"Local\XamlTest.ResourceLocks.Focus")
        ];

        private readonly ManualResetEventSlim _releaseSignal = new(false);
        private readonly TaskCompletionSource _acquired = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _released = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _disposeSignaled;

        private ResourceLockLease()
        {
            _acquired.SetResult();
            _released.SetResult();
        }

        private ResourceLockLease(IReadOnlyList<string> lockNames, TimeSpan timeout, CancellationToken cancellationToken)
        {
            Thread ownerThread = new(() => AcquireAndHoldLocks(lockNames, timeout, cancellationToken))
            {
                IsBackground = true,
                Name = "XamlTest.ResourceLockLease"
            };
            ownerThread.Start();
        }

        public static async ValueTask<IAsyncDisposable> Acquire(
            ResourceLocks requestedLocks,
            TimeSpan timeout,
            CancellationToken cancellationToken)
        {
            if (timeout < TimeSpan.Zero && timeout != Timeout.InfiniteTimeSpan)
            {
                throw new ArgumentOutOfRangeException(nameof(timeout), timeout, "Timeout must be non-negative or Timeout.InfiniteTimeSpan.");
            }

            var locks = GetRequestedLocks(requestedLocks);
            if (locks.Count == 0)
            {
                return Empty;
            }

            ResourceLockLease lease = new(locks, timeout, cancellationToken);
            await lease._acquired.Task.ConfigureAwait(false);
            return lease;
        }

        private void AcquireAndHoldLocks(IReadOnlyList<string> lockNames, TimeSpan timeout, CancellationToken cancellationToken)
        {
            List<Mutex> acquiredMutexes = new(lockNames.Count);
            Stopwatch elapsed = Stopwatch.StartNew();
            try
            {
                foreach (var lockName in lockNames)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    TimeSpan remaining = GetRemainingTimeout(timeout, elapsed);
                    Mutex mutex = new(false, lockName);
                    if (!Wait(mutex, remaining, cancellationToken))
                    {
                        mutex.Dispose();
                        throw new TimeoutException($"Failed to acquire resource lock '{lockName}' within {timeout}.");
                    }

                    acquiredMutexes.Add(mutex);
                }

                _acquired.SetResult();
                _releaseSignal.Wait();
            }
            catch (Exception ex)
            {
                _acquired.TrySetException(ex);
            }
            finally
            {
                ReleaseAll(acquiredMutexes);
                _released.TrySetResult();
            }
        }

        private static bool Wait(Mutex mutex, TimeSpan timeout, CancellationToken cancellationToken)
        {
            if (!cancellationToken.CanBeCanceled)
            {
                try
                {
                    return mutex.WaitOne(timeout);
                }
                catch (AbandonedMutexException)
                {
                    return true;
                }
            }

            int waitResult;
            try
            {
                waitResult = WaitHandle.WaitAny([mutex, cancellationToken.WaitHandle], timeout);
            }
            catch (AbandonedMutexException ex) when (ex.MutexIndex == 0)
            {
                return true;
            }

            return waitResult switch
            {
                WaitHandle.WaitTimeout => false,
                0 => true,
                1 => throw new OperationCanceledException(cancellationToken),
                _ => throw new InvalidOperationException($"Unexpected wait result '{waitResult}'.")
            };
        }

        private static TimeSpan GetRemainingTimeout(TimeSpan timeout, Stopwatch elapsed)
        {
            if (timeout == Timeout.InfiniteTimeSpan)
            {
                return timeout;
            }

            TimeSpan remaining = timeout - elapsed.Elapsed;
            return remaining > TimeSpan.Zero ? remaining : TimeSpan.Zero;
        }

        private static IReadOnlyList<string> GetRequestedLocks(ResourceLocks requestedLocks)
        {
            ResourceLocks supportedLocks = ResourceLocks.All;
            ResourceLocks invalidFlags = requestedLocks & ~supportedLocks;
            if (invalidFlags != ResourceLocks.None)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedLocks), requestedLocks, $"Unknown resource lock flags '{invalidFlags}'.");
            }

            return OrderedLocks
                .Where(x => requestedLocks.HasFlag(x.Lock))
                .Select(x => x.Name)
                .ToList();
        }

        public ValueTask DisposeAsync()
        {
            if (Interlocked.Exchange(ref _disposeSignaled, 1) == 0)
            {
                _releaseSignal.Set();
                return new ValueTask(_released.Task);
            }

            return ValueTask.CompletedTask;
        }

        private static void ReleaseAll(IReadOnlyList<Mutex> acquiredMutexes)
        {
            for (int i = acquiredMutexes.Count - 1; i >= 0; i--)
            {
                Mutex mutex = acquiredMutexes[i];
                mutex.ReleaseMutex();
                mutex.Dispose();
            }
        }
    }
}
