using System.Threading.Tasks;

namespace XamlTest
{
    public static class WindowMixins
    {
        public static Task WaitForLoaded(this IWindow window) 
            => Wait.For(async () => await window.GetIsLoaded());

        public static async Task<bool> GetIsLoaded(this IWindow window)
            => await window.GetProperty<bool>("IsLoaded");

        public static async Task<string> GetTitle(this IWindow window)
            => await window.GetProperty<string>("Title");
    }
}
