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

    static HighlightConfig()
    {
        Brush borderBrush = new SolidColorBrush(DefaultBorderColor);
        borderBrush.Freeze();
        Brush overlayBrush = new SolidColorBrush(DefaultOverlayColor);
        overlayBrush.Freeze();

        Default = new()
        {
            IsVisible = true,
            BorderBrush = borderBrush,
            BorderThickness = DefaultBorderWidth,
            OverlayBrush = overlayBrush
        };
    }

    public static HighlightConfig Default { get; }

    public static HighlightConfig None { get; } = new()
    {
        IsVisible = false
    };
}
