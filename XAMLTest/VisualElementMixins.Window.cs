namespace XamlTest;

public static partial class VisualElementMixins
{
    public static async Task<IVisualElement> SetXamlContent(this IWindow window, XamlSegment xaml)
    {
        if (xaml is null)
        {
            throw new ArgumentNullException(nameof(xaml));
        }
#if WPF
        await using var layout = await window.RegisterForEvent(nameof(Window.ContentRendered));
#endif
        IVisualElement element = await window.SetXamlProperty(nameof(Window.Content), xaml);
#if WPF
        await Wait.For(async () => (await layout.GetInvocations()).Any());
#endif
        return element;
    }

    public static async Task<IVisualElement<TElement>> SetXamlContent<TElement>(this IWindow window, XamlSegment xaml)
    {
        if (xaml is null)
        {
            throw new ArgumentNullException(nameof(xaml));
        }
#if WPF
        await using var layout = await window.RegisterForEvent(nameof(Window.ContentRendered));
#endif
        IVisualElement<TElement> element = await window.SetXamlProperty<TElement>(nameof(Window.Content), xaml);
#if WPF
        await Wait.For(async () => (await layout.GetInvocations()).Any());
#endif
        return element;
    }
}
