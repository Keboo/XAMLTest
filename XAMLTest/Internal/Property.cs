using System;

namespace XamlTest.Internal
{

    internal class Property : BaseValue, IProperty
    {
        public string PropertyType { get; }

        public Property(string propertyType, string valueType, string value, Serializer serializer)
            : base(valueType, value, serializer)
        {
            PropertyType = propertyType ?? throw new ArgumentNullException(nameof(propertyType));
        }
    }
}
