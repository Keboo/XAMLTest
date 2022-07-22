#if WIN_UI
using System.Collections.Concurrent;
using WinRT;
#endif

namespace XamlTest.Internal;

internal static class DependencyObjectTracker
{
#if WPF
    private static string GetId(DependencyObject obj) => (string)obj.GetValue(IdProperty);

    private static void SetId(DependencyObject obj, string value) => obj.SetValue(IdProperty, value);

    // Using a DependencyProperty as the backing store for Id.  This enables animation, styling, binding, etc...
    public static readonly DependencyProperty IdProperty =
        DependencyProperty.RegisterAttached("Id", typeof(string), typeof(DependencyObjectTracker), new PropertyMetadata(""));

    internal static string GetOrSetId(DependencyObject obj, IDictionary<string, WeakReference<DependencyObject>> cache)
    {
        string id = GetId(obj);
        if (string.IsNullOrWhiteSpace(id))
        {
            SetId(obj, id = Guid.NewGuid().ToString());
        }
        lock (cache)
        {
            cache[id] = new WeakReference<DependencyObject>(obj);
        }
        return id;
    }
#endif

#if WIN_UI
    private static ObjectMap Map { get; } = new();

    internal static string GetOrSetId(IWinRTObject obj, IDictionary<string, WeakReference<IWinRTObject>> cache)
    {
        string id = Map.SetOrGetExistingId(obj, Guid.NewGuid().ToString());
        lock (cache)
        {
            cache[id] = new WeakReference<IWinRTObject>(obj);
        }
        return id;
    }

    private class ObjectMap
    {
        private Dictionary<WeakReference<IWinRTObject>, string> Map { get; } = new();

        public string SetOrGetExistingId(IWinRTObject @object, string newId)
        {
            lock(Map)
            {
                foreach(var (key, value) in Map)
                {
                    if (key.TryGetTarget(out IWinRTObject? existingObject) &&
                        existingObject.Equals(@object) == true)
                    {
                        return value;
                    }
                }
                Map[new WeakReference<IWinRTObject>(@object)] = newId;
                return newId;
            }
        }
    }
#endif
}
