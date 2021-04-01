using System.Threading.Tasks;
using System.Windows;

namespace XamlTest
{
    public static class WindowMixins
    {
        public static Task WaitForLoaded(this IWindow window) 
            => Wait.For(async () => await window.GetIsLoaded());

        public static async Task<bool> GetIsLoaded(this IWindow window)
            => await window.GetProperty<bool>(nameof(Window.IsLoaded));

        public static async Task<string?> GetTitle(this IWindow window)
            => await window.GetProperty<string?>(nameof(Window.Title));

        public static async Task<string?> SetTitle(this IWindow window, string? title)
            => await window.SetProperty(nameof(Window.Title), title);
    }
}
