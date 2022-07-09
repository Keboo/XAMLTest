using WinRT;

namespace XamlTest.Internal;

internal static class DependencyObjectTracker
{
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

#if WIN_UI
    private static readonly string XamlTestId = $"XamlTest-{Guid.NewGuid()}";

    internal static string GetOrSetId(NativeWindow obj, IDictionary<string, WeakReference<IWinRTObject>> cache)
    {
        string id = "";
        if (!obj.CoreWindow.CustomProperties.TryGetValue(XamlTestId, out object? objId) && 
            objId is string stringId)
        {
            id = stringId;
        }
        if (string.IsNullOrWhiteSpace(id))
        {
            obj.CoreWindow.CustomProperties[XamlTestId] = id = Guid.NewGuid().ToString();
        }
        lock (cache)
        {
            cache[id] = new WeakReference<IWinRTObject>(obj);
        }
        return id;
    }
#endif
}
