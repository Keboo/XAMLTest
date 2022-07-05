namespace XamlTest;

public sealed class HighlightConfig
{
    public static Color DefaultBorderColor { get; } = Color.FromArgb(0x75, 0xFF, 0x0, 0x0);
    public const double DefaultBorderWidth = 5.0;
    public static Color DefaultOverlayColor { get; } = Color.FromArgb(0x30, 0xFF, 0x0, 0x0);

    public bool IsVisible { get; set; } = true;

    public Color? BorderColor { get; set; }
    public double BorderThickness { get; set; }
    public Color? OverlayColor { get; set; }

    static HighlightConfig()
    {
        Default = new()
        {
            IsVisible = true,
            BorderColor = DefaultBorderColor,
            BorderThickness = DefaultBorderWidth,
            OverlayColor = DefaultOverlayColor
        };
    }

    public static HighlightConfig Default { get; }

    public static HighlightConfig None { get; } = new()
    {
        IsVisible = false
    };
}
