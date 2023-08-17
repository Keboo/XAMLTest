namespace XamlTest;

static partial class VisualElementMixins
{
    public static Task<IVisualElement<TElement>> GetElement<TElement>(
        this IVisualElement element,
        IQuery<TElement> query) 
        => element.GetElement<TElement>(query.QueryString);

    public static Task<IVisualElement<TElement>> GetElement<TElement>(
        this IVisualElement element)
        => element.GetElement(ElementQuery.OfType<TElement>());

    public static Task<IVisualElement<TElement>?> FindElement<TElement>(
        this IVisualElement element,
        IQuery<TElement> query)
        => element.FindElement<TElement>(query.QueryString);

    public static Task<IVisualElement<TElement>?> FindElement<TElement>(
        this IVisualElement element)
        => element.FindElement(ElementQuery.OfType<TElement>());
}
