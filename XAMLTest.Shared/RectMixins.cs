using System.Runtime.CompilerServices;

namespace XamlTest;

internal static class RectMixins
{
    public static Point Center(this Rect rect) 
        => NewPoint(rect.Left + rect.Width / 2.0, rect.Top + rect.Height / 2.0);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static Point NewPoint(double x, double y)
    {
#if WPF
        return new Point(x, y);
#elif WIN_UI
        return new Point((float)x, (float)y);
#endif
    }
}
