using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace XamlTest.Transport
{
    internal class GridSerializer : ISerializer
    {
        public bool CanSerialize(Type type)
            => type == typeof(IList<ColumnDefinition>) ||
               type == typeof(IList<RowDefinition>) ||
               type == typeof(ColumnDefinition) ||
               type == typeof(RowDefinition);

        public object? Deserialize(Type type, string value)
        {
            if (type == typeof(IList<ColumnDefinition>))
            {
                List<ColumnDefinition> rv = new();
                foreach(var data in JsonSerializer.Deserialize<List<ColumnDefinitionData>>(value))
                {
                    rv.Add(ConvertFrom(data));
                }
                return rv;
            }
            if (type == typeof(ColumnDefinition))
            {
                var data = JsonSerializer.Deserialize<ColumnDefinitionData>(value);
                return ConvertFrom(data);
            }
            return null;
        }
        public string Serialize(Type type, object? value)
        {
            if (type == typeof(IList<ColumnDefinition>) &&
                value is IList<ColumnDefinition> columnDefinitions)
            {
                return JsonSerializer.Serialize(ConvertTo(columnDefinitions));
            }
            if (type == typeof(ColumnDefinition) &&
                value is ColumnDefinition column)
            {
                return JsonSerializer.Serialize(ConvertTo(column));
            }
            return "";
        }

        private static GridLength ConvertFrom(GridLengthData? value)
            => value is null ? default : new(value.Value, value.GridUnitType);

        private static GridLengthData? ConvertTo(GridLength? value)
        {
            if (value is null) return null;
            return new()
            {
                GridUnitType = value.Value.GridUnitType,
                Value = value.Value.Value
            };
        }

        private static ColumnDefinition ConvertFrom(ColumnDefinitionData value)
            => new()
            {
                MinWidth = value.MinWidth,
                MaxWidth = value.MaxWidth,
                Width = ConvertFrom(value?.Width)
            };

        private static ColumnDefinitionData? ConvertTo(ColumnDefinition? value)
            => value is null ? default : new()
            {
                MinWidth = value.MinWidth,
                MaxWidth = value.MaxWidth,
                Width = ConvertTo(value.Width)
            };

        private static List<ColumnDefinitionData> ConvertTo(IList<ColumnDefinition> value) 
            => value.Select(x => ConvertTo(x)).OfType<ColumnDefinitionData>().ToList();
    }

    internal class ColumnDefinitionData
    {
        public GridLengthData? Width { get; set; }
        public double MinWidth { get; set; }
        public double MaxWidth { get; set; }
    }

    internal class GridLengthData
    {
        public double Value { get; set; }
        public GridUnitType GridUnitType { get; set; }
    }
}
