namespace XamlTest;

public record struct Area(double Left, double Top, double Right, double Bottom)
{
    public static Area Empty { get; } = new();

    public Location TopLeft => new(Left, Top);
    public Location TopRight => new(Right, Top);
    public Location BottomRight => new(Right, Bottom);
    public Location BottomLeft => new(Left, Bottom);
    public double Width => Right - Left;
    public double Height => Bottom - Top;

    public Location Center()
        => new(Left + Width / 2, Top + Height / 2);
}
