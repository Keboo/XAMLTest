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

    public static Task<IReadOnlyList<IVisualElement<TElement>>> GetElements<TElement>(
        this IVisualElement element,
        IQuery<TElement> query)
        => element.GetElements<TElement>(query.QueryString);

    public static Task<IReadOnlyList<IVisualElement<TElement>>> GetElements<TElement>(
        this IVisualElement element)
        => element.GetElements(ElementQuery.OfType<TElement>());
}
