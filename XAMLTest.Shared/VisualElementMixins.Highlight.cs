using System.Threading.Tasks;

namespace XamlTest;

public static partial class VisualElementMixins
{
    /// <summary>
    /// Clears any exisitng highlight from a control.
    /// </summary>
    /// <returns></returns>
    public static Task ClearHighlight(this IVisualElement element)
        => element.Highlight(HighlightConfig.None);

    /// <summary>
    /// Applys a highlight with the default configuration.
    /// </summary>
    /// <param name="element">The element to highlight</param>
    /// <returns></returns>
    public static Task Highlight(this IVisualElement element) 
        => element.Highlight(HighlightConfig.Default);
}
