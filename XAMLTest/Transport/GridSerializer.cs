using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;

namespace XamlTest.Transport
{
    public class GridSerializer : ISerializer
    {
        public bool CanSerialize(Type type)
            => typeof(IEnumerable<ColumnDefinition>).IsAssignableFrom(type) ||
               typeof(IEnumerable<RowDefinition>).IsAssignableFrom(type) ||
               type == typeof(ColumnDefinition) ||
               type == typeof(RowDefinition);

        public object? Deserialize(Type type, string value)
        {
            if (typeof(IEnumerable<ColumnDefinition>).IsAssignableFrom(type))
            {
                List<ColumnDefinition> rv = new();
                foreach(var data in JsonSerializer.Deserialize<List<ColumnDefinitionData>>(value) ?? Enumerable.Empty<ColumnDefinitionData >())
                {
                    rv.Add(ConvertFrom(data));
                }
                return rv;
            }
            if (typeof(IEnumerable<RowDefinition>).IsAssignableFrom(type))
            {
                List<RowDefinition> rv = new();
                foreach (var data in JsonSerializer.Deserialize<List<RowDefinitionData>>(value) ?? Enumerable.Empty<RowDefinitionData>())
                {
                    rv.Add(ConvertFrom(data));
                }
                return rv;
            }
            if (type == typeof(ColumnDefinition))
            {
                var data = JsonSerializer.Deserialize<ColumnDefinitionData>(value);
                return data is null ? null : ConvertFrom(data);
            }
            if (type == typeof(RowDefinition))
            {
                var data = JsonSerializer.Deserialize<RowDefinitionData>(value);
                return data is null ? null : ConvertFrom(data);
            }
            return null;
        }
        public string Serialize(Type type, object? value)
        {
            if (typeof(IEnumerable<ColumnDefinition>).IsAssignableFrom(type) &&
                value is IEnumerable<ColumnDefinition> columnDefinitions)
            {
                return JsonSerializer.Serialize(ConvertTo(columnDefinitions));
            }
            if (typeof(IEnumerable<RowDefinition>).IsAssignableFrom(type) &&
                value is IEnumerable<RowDefinition> rowDefinitions)
            {
                return JsonSerializer.Serialize(ConvertTo(rowDefinitions));
            }
            if (type == typeof(ColumnDefinition) &&
                value is ColumnDefinition column)
            {
                return JsonSerializer.Serialize(ConvertTo(column));
            }
            if (type == typeof(ColumnDefinition) &&
                value is RowDefinition row)
            {
                return JsonSerializer.Serialize(ConvertTo(row));
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

        private static List<ColumnDefinitionData> ConvertTo(IEnumerable<ColumnDefinition> value) 
            => value.Select(x => ConvertTo(x)).OfType<ColumnDefinitionData>().ToList();

        private static RowDefinition ConvertFrom(RowDefinitionData value)
            => new()
            {
                MinHeight = value.MinHeight,
                MaxHeight = value.MaxHeight,
                Height = ConvertFrom(value?.Height)
            };

        private static RowDefinitionData? ConvertTo(RowDefinition? value)
            => value is null ? default : new()
            {
                MinHeight = value.MinHeight,
                MaxHeight = value.MaxHeight,
                Height = ConvertTo(value.Height)
            };

        private static List<RowDefinitionData> ConvertTo(IEnumerable<RowDefinition> value)
            => value.Select(x => ConvertTo(x)).OfType<RowDefinitionData>().ToList();
    }

    internal class ColumnDefinitionData
    {
        public GridLengthData? Width { get; set; }
        public double MinWidth { get; set; }
        public double MaxWidth { get; set; }
    }

    internal class RowDefinitionData
    {
        public GridLengthData? Height { get; set; }
        public double MinHeight { get; set; }
        public double MaxHeight { get; set; }
    }

    internal class GridLengthData
    {
        public double Value { get; set; }
        public GridUnitType GridUnitType { get; set; }
    }
}
