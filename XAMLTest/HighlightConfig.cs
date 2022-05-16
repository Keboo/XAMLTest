using System.Windows.Media;

namespace XamlTest;

public sealed class HighlightConfig
{
    public static Color DefaultBorderColor { get; } = Color.FromArgb(0x75, 0xFF, 0x0, 0x0);
    public const double DefaultBorderWidth = 5.0;
    public static Color DefaultOverlayColor { get; } = Color.FromArgb(0x30, 0xFF, 0x0, 0x0);

    public bool IsVisible { get; set; } = true;

    public Brush? BorderBrush { get; set; }
    public double BorderThickness { get; set; }
    public Brush? OverlayBrush { get; set; }

    public static HighlightConfig Default { get; } = new()
    {
        IsVisible = true,
        BorderBrush = new SolidColorBrush(DefaultBorderColor),
        BorderThickness = DefaultBorderWidth,
        OverlayBrush = new SolidColorBrush(DefaultOverlayColor)
    };

    public static HighlightConfig None { get; } = new()
    {
        IsVisible = false
    };
}
