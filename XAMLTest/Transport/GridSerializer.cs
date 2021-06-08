using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Controls;

namespace XamlTest.Transport
{
    internal class GridSerializer : ISerializer
    {
        public bool CanSerialize(Type type)
            => type == typeof(ColumnDefinitionCollection) ||
               type == typeof(RowDefinitionCollection) ||
               type == typeof(ColumnDefinition) ||
               type == typeof(RowDefinition);

        public object? Deserialize(Type type, string value) => throw new NotImplementedException();
        public string Serialize(Type type, object? value)
        {

        }
    }
}
