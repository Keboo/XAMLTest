using System;
using System.Diagnostics.CodeAnalysis;

namespace XamlTest.Internal
{
    internal class Property : BaseValue, IProperty
    {
        public string PropertyType { get; }

        public IVisualElement? Element { get; }

        public Property(string propertyType, string valueType, object? value, IVisualElement? element, 
            Serializer serializer)
            : base(valueType, value, serializer)
        {
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
            Element = element;
        }

        [return: MaybeNull]
        public override T GetAs<T>()
        {
            if (Element is T typedElement)
            {
                return typedElement;
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
}
