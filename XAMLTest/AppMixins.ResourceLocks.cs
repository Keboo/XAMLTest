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

    private sealed class ResourceLockLease(IReadOnlyList<SemaphoreSlim> acquiredSemaphores) : IAsyncDisposable
    {
        private IReadOnlyList<SemaphoreSlim>? AcquiredSemaphores { get; set; } = acquiredSemaphores;

        private static IAsyncDisposable Empty { get; } = new ResourceLockLease([]);

        private static IReadOnlyList<ResourceLocks> OrderedLocks { get; } =
        [
            ResourceLocks.Keyboard,
            ResourceLocks.Mouse,
            ResourceLocks.Focus
        ];

        private static SemaphoreSlim KeyboardSemaphore { get; } = new(1, 1);
        private static SemaphoreSlim MouseSemaphore { get; } = new(1, 1);
        private static SemaphoreSlim FocusSemaphore { get; } = new(1, 1);

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

            List<SemaphoreSlim> acquiredSemaphores = new(locks.Count);
            try
            {
                foreach (var requestedLock in locks)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    SemaphoreSlim semaphore = GetSemaphore(requestedLock);
                    if (!await semaphore.WaitAsync(timeout, cancellationToken).ConfigureAwait(false))
                    {
                        throw new TimeoutException($"Failed to acquire resource lock '{requestedLock}' within {timeout}.");
                    }

                    acquiredSemaphores.Add(semaphore);
                }
            }
            catch
            {
                ReleaseAll(acquiredSemaphores);
                throw;
            }

            return new ResourceLockLease(acquiredSemaphores);

            static void ReleaseAll(IReadOnlyList<SemaphoreSlim> acquiredSemaphores)
            {
                for (int i = acquiredSemaphores.Count - 1; i >= 0; i--)
                {
                    acquiredSemaphores[i].Release();
                }
            }
        }

        private static IReadOnlyList<ResourceLocks> GetRequestedLocks(ResourceLocks requestedLocks)
        {
            ResourceLocks supportedLocks = ResourceLocks.All;
            ResourceLocks invalidFlags = requestedLocks & ~supportedLocks;
            if (invalidFlags != ResourceLocks.None)
            {
                throw new ArgumentOutOfRangeException(nameof(requestedLocks), requestedLocks, $"Unknown resource lock flags '{invalidFlags}'.");
            }

            return OrderedLocks
                .Where(x => requestedLocks.HasFlag(x))
                .ToList();
        }

        private static SemaphoreSlim GetSemaphore(ResourceLocks requestedLock)
            => requestedLock switch
            {
                ResourceLocks.Keyboard => KeyboardSemaphore,
                ResourceLocks.Mouse => MouseSemaphore,
                ResourceLocks.Focus => FocusSemaphore,
                _ => throw new ArgumentOutOfRangeException(nameof(requestedLock), requestedLock, "Unsupported lock.")
            };

        public ValueTask DisposeAsync()
        {
            if (AcquiredSemaphores is { } acquiredSemaphores)
            {
                AcquiredSemaphores = null;
                ReleaseAll(acquiredSemaphores);
            }
            return ValueTask.CompletedTask;

            static void ReleaseAll(IReadOnlyList<SemaphoreSlim> acquiredSemaphores)
            {
                for (int i = acquiredSemaphores.Count - 1; i >= 0; i--)
                {
                    acquiredSemaphores[i].Release();
                }
            }
        }
    }
}
