using System;
using System.Threading.Tasks;
using System.Windows;

namespace XamlTest
{
    public static partial class VisualElementMixins
    {
        public static async Task SetXamlContent(this IWindow window, string xaml)
        {
            if (xaml is null)
            {
                throw new ArgumentNullException(nameof(xaml));
            }

            await window.SetProperty(nameof(Window.Content), xaml, Types.XamlString);
        }
    }
}
