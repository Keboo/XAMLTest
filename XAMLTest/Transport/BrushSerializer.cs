using System.Collections.Generic;

namespace XamlTest.Transport;

public class BrushSerializer : ISerializer
{
    public bool CanSerialize(Type type)
        => type == typeof(Brush) ||
           type == typeof(LinearGradientBrush) ||
           type == typeof(RadialGradientBrush) ||
           type == typeof(SolidColorBrush) ||
           type == typeof(Color) ||
           type == typeof(Color?);

    public object? Deserialize(Type type, string value)
    {
        if (type == typeof(LinearGradientBrush))
        {
            if (!string.IsNullOrEmpty(value) &&
                System.Text.Json.JsonSerializer.Deserialize<BrushData>(value) is { } brushData)
            {
                return brushData.LinearGradientData?.GetBrush();
            }
        }
        else if (type == typeof(RadialGradientBrush))
        {
            if (!string.IsNullOrEmpty(value) &&
                System.Text.Json.JsonSerializer.Deserialize<BrushData>(value) is { } brushData)
            {
                return brushData.RadialGradientData?.GetBrush();
            }
        }
        else if (type == typeof(SolidColorBrush))
        {
            if (!string.IsNullOrEmpty(value) &&
                System.Text.Json.JsonSerializer.Deserialize<BrushData>(value) is { } brushData)
            {
                return brushData.SolidColorData?.GetBrush();
            }
        }
        else if (type == typeof(Brush))
        {
            if (string.IsNullOrEmpty(value)) return null;
            if (System.Text.Json.JsonSerializer.Deserialize<BrushData>(value) is { } brushData)
            {
                if (brushData.SolidColorData is not null)
                {
                    return brushData.SolidColorData.GetBrush();
                }
                if (brushData.LinearGradientData is not null)
                {
                    return brushData.LinearGradientData.GetBrush();
                }
                if (brushData.RadialGradientData is not null)
                {
                    return brushData.RadialGradientData.GetBrush();
                }
            }
        }
        else if (type == typeof(Color))
        {
            if (!string.IsNullOrEmpty(value) &&
                System.Text.Json.JsonSerializer.Deserialize<BrushData>(value) is { } brushData &&
                brushData.SolidColorData is { } data)
            {
                return data.Color;
            }
            return default(Color);
        }
        else if (type == typeof(Color?))
        {
            if (!string.IsNullOrEmpty(value) &&
                System.Text.Json.JsonSerializer.Deserialize<BrushData>(value) is { } brushData &&
                brushData.SolidColorData is { } data)
            {
                return data.Color;
            }
        }
        return null;
    }

    public string Serialize(Type type, object? value)
    {
        return value switch
        {
            SolidColorBrush brush => System.Text.Json.JsonSerializer.Serialize((BrushData)brush),
            LinearGradientBrush linearBrush => System.Text.Json.JsonSerializer.Serialize((BrushData)linearBrush),
            RadialGradientBrush radialBrush => System.Text.Json.JsonSerializer.Serialize((BrushData)radialBrush),
            Color color => System.Text.Json.JsonSerializer.Serialize((BrushData)color),
            _ => ""
        };
    }

    private class BrushData
    {
        public SolidColorBrushData? SolidColorData { get; set; }
        public LinearGradientBrushData? LinearGradientData { get; set; }
        public RadialGradientBrushData? RadialGradientData { get; set; }

        public static implicit operator BrushData(SolidColorBrush brush)
            => new()
            {
                SolidColorData = new(brush)
            };

        public static implicit operator BrushData(Color color)
            => new()
            {
                SolidColorData = new(color)
            };

        public static implicit operator BrushData(LinearGradientBrush brush) 
            => new()
            {
                LinearGradientData = new(brush)
            };

        public static implicit operator BrushData(RadialGradientBrush brush)
            => new()
            {
                RadialGradientData = new(brush)
            };
    }

    private class SolidColorBrushData
    {
        public Color Color { get; set; }
        public double Opacity { get; set; }

        public SolidColorBrushData()
        { }

        public SolidColorBrushData(Color color)
        {
            Color = color;
        }

        public SolidColorBrushData(SolidColorBrush brush)
        {
            Color = brush.Color;
            Opacity = brush.Opacity;
        }

        public SolidColorBrush GetBrush()
        {
            return new SolidColorBrush
            {
                Color = Color,
                Opacity = Opacity
            };
        }
    }

    private abstract class GradientBrushData
    {
        public List<GradientStopData>? GradientStops { get; set; }

        protected GradientBrushData()
        {

        }

        protected GradientBrushData(IEnumerable<GradientStop> gradientStops)
        {
            GradientStops = gradientStops
                .Select(x => new GradientStopData
                {
                    Color = x.Color,
                    Offset = x.Offset
                }).ToList();
        }
    }

    private class GradientStopData
    {
        public Color Color { get; set; }
        public double Offset { get; set; }
    }

    private class LinearGradientBrushData : GradientBrushData
    {
        public Point StartPoint { get; set; }
        public Point EndPoint { get; set; }

        public LinearGradientBrushData()
        {
        }

        public LinearGradientBrushData(LinearGradientBrush brush)
            : base(brush.GradientStops)
        {
            StartPoint = brush.StartPoint;
            EndPoint = brush.EndPoint;
            GradientStops = brush.GradientStops
                .OfType<GradientStop>()
                .Select(x => new GradientStopData
                {
                    Color = x.Color,
                    Offset = x.Offset
                }).ToList();
        }

        public LinearGradientBrush GetBrush()
        {
            var brush = new LinearGradientBrush
            {
                StartPoint = StartPoint,
                EndPoint = EndPoint,
            };
#if WPF
            var gradientStops = GradientStops?.Select(x => new GradientStop(x.Color, x.Offset)) ?? Enumerable.Empty<GradientStop>();
            brush.GradientStops = new GradientStopCollection(gradientStops);
#elif WIN_UI
            foreach (var stop in GradientStops ?? Enumerable.Empty<GradientStopData>())
            {
                brush.GradientStops.Add(new GradientStop
                {
                    Color = stop.Color,
                    Offset = stop.Offset
                });
            }
#endif
            return brush;
        }
    }

    private class RadialGradientBrushData : GradientBrushData
    {
        public Point Center { get; set; }
        public double RadiusX { get; set; }
        public double RadiusY { get; set; }
        public Point GradientOrigin { get; set; }
        public GradientSpreadMethod SpreadMethod { get; set; }

        public RadialGradientBrushData()
        {
        }

        public RadialGradientBrushData(RadialGradientBrush brush)
            : base(brush.GradientStops)
        {
            Center = brush.Center;
            RadiusX = brush.RadiusX;
            RadiusY = brush.RadiusY;
            GradientOrigin = brush.GradientOrigin;
        }

        public RadialGradientBrush GetBrush()
        {
            var brush = new RadialGradientBrush
            {
                Center = Center,
                RadiusX = RadiusX,
                RadiusY = RadiusY,
                GradientOrigin = GradientOrigin,
                SpreadMethod = SpreadMethod
            };
#if WPF
            var gradientStops = GradientStops?.Select(x => new GradientStop(x.Color, x.Offset)) ?? Enumerable.Empty<GradientStop>();
            brush.GradientStops = new GradientStopCollection(gradientStops);
#elif WIN_UI
            foreach(var stop in GradientStops ?? Enumerable.Empty<GradientStopData>())
            {
                brush.GradientStops.Add(new GradientStop
                {
                    Color = stop.Color,
                    Offset = stop.Offset
                });
            }
#endif
            return brush;
        }
    }
}
