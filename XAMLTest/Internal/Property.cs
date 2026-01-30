namespace XamlTest.Internal;

internal class Property(string propertyType, string valueType, object? value, IVisualElement? element,
    AppContext context) : BaseValue(valueType, value, context), IProperty
{
    public string PropertyType { get; } = propertyType ?? throw new ArgumentNullException(nameof(propertyType));

    public IVisualElement? Element { get; } = element;

    [return: MaybeNull]
    public override T GetAs<T>()
    {
        if (Element is T typedElement)
        {
            return typedElement;
        }
        if (Element is not null && typeof(T) is var genericType && 
            genericType.IsGenericType && genericType.GetGenericTypeDefinition() == typeof(IVisualElement<>))
        {
            return (T)Element.As(genericType);
        }
        Type desiredType = typeof(T);
        if (string.IsNullOrEmpty(Value?.ToString()) &&
            (desiredType == typeof(IVisualElement) || typeof(IVisualElement).IsAssignableFrom(desiredType)))
        {
            return default;
        }
        return base.GetAs<T>();
    }
}
