using System.Windows.Controls;

namespace XamlTest;

public static partial class VisualElementMixins
{
    public static async Task<IVisualElement> SetXamlContent(this IWindow window, XamlSegment xaml)
    {
        if (window is null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        if (xaml is null)
        {
            throw new ArgumentNullException(nameof(xaml));
        }
        await using var layout = await window.RegisterForEvent(nameof(Window.ContentRendered));
        IVisualElement element = await window.SetXamlProperty(nameof(Window.Content), xaml);
        await Wait.For(async () => (await layout.GetInvocations()).Any());
        return element;
    }

    public static async Task<IVisualElement<TElement>> SetXamlContent<TElement>(this IWindow window, XamlSegment xaml)
    {
        if (window is null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        if (xaml is null)
        {
            throw new ArgumentNullException(nameof(xaml));
        }
        await using var layout = await window.RegisterForEvent(nameof(Window.ContentRendered));
        IVisualElement<TElement> element = await window.SetXamlProperty<TElement>(nameof(Window.Content), xaml);
        await Wait.For(async () => (await layout.GetInvocations()).Any());
        return element;
    }

    public static async Task<IVisualElement<TUserControl>> SetXamlContentFromUserControl<TUserControl>(this IWindow window)
        where TUserControl : UserControl
    {
        if (window is null)
        {
            throw new ArgumentNullException(nameof(window));
        }

        XamlSegment segment = new($"<local:{typeof(TUserControl).Name} />",
            new XmlNamespace("local", $"clr-namespace:{typeof(TUserControl).Namespace};assembly={typeof(TUserControl).Assembly.GetName().Name}"));
        return await window.SetXamlContent<TUserControl>(segment);
    }
}
