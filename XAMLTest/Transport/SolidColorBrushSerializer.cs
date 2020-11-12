using System;
using System.Text.Json;
using System.Windows.Media;

namespace XamlTest.Transport
{
    public class SolidColorBrushSerializer : ISerializer
    {
        public bool CanSerialize(Type type)
            => type == typeof(SolidColorBrush) ||
               type == typeof(Color) ||
               type == typeof(Color?);

        public object? Deserialize(Type type, string value)
        {
            if (type == typeof(SolidColorBrush))
            {
                if (!string.IsNullOrWhiteSpace(value) &&
                    JsonSerializer.Deserialize<BrushData>(value) is { } brushData)
                {
                    return (SolidColorBrush)brushData;
                }
            }
            else if (type == typeof(Color))
            {
                if (JsonSerializer.Deserialize<BrushData>(value) is { } brushData)
                {
                    return (Color)brushData;
                }
                return default(Color);
            }
            else if (type == typeof(Color?))
            {
                if (!string.IsNullOrWhiteSpace(value) &&
                    JsonSerializer.Deserialize<BrushData>(value) is { } brushData)
                {
                    return (Color)brushData;
                }
            }
            return null;
        }

        public string Serialize(Type type, object? value)
        {
            return value switch
            {
                SolidColorBrush brush => JsonSerializer.Serialize((BrushData)brush),
                Color color => JsonSerializer.Serialize((BrushData)color),
                _ => ""
            };
        }

        private class BrushData
        {
            public Color Color { get; set; }
            public double Opacity { get; set; }

            public static implicit operator SolidColorBrush(BrushData data)
                => new SolidColorBrush(data.Color)
                {
                    Opacity = data.Opacity
                };

            public static implicit operator Color(BrushData data)
                => data.Color;

            public static implicit operator BrushData(SolidColorBrush brush)
                => new BrushData
                {
                    Color = brush.Color,
                    Opacity = brush.Opacity
                };

            public static implicit operator BrushData(Color color)
                => new BrushData
                {
                    Color = color
                };
        }
    }
}
