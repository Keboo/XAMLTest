using System.Windows;

namespace XamlTest
{
    internal static class RectMixins
    {
        public static Point Center(this Rect rect) 
            => new(rect.Left + rect.Width / 2, rect.Top + rect.Height / 2);
    }
}
