using System;
using System.Collections.Generic;
using System.Linq;
using XamlTest.Transport;

namespace XamlTest.Internal
{
    internal class Serializer
    {
        public List<ISerializer> Serializers { get; } = new List<ISerializer>();

        public Serializer()
        {
            //NB: Order matters here. Items earlier in the list take precedence
            Serializers.Add(new SolidColorBrushSerializer());
            Serializers.Add(new DefaultSerializer());
        }

        public void AddSerializer(ISerializer serializer, int index) 
            => Serializers.Insert(index, serializer);

        public string? Serialize(Type type, object? value)
        {
            if (Serializers.FirstOrDefault(x => x.CanSerialize(type)) is { } serializer)
            {
                return serializer.Serialize(type, value);
            }
            return null;
        }

        public object? Deserialize(Type type, string value)
        {
            if (Serializers.FirstOrDefault(x => x.CanSerialize(type)) is { } serializer)
            {
                return serializer.Deserialize(type, value);
            }
            return null;
        }
    }
}
