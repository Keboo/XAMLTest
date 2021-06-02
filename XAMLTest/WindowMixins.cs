﻿using System.Threading.Tasks;
using System.Windows;

namespace XamlTest
{
    public static class WindowMixins
    {
        public static Task WaitForLoaded(this IWindow window) 
            => Wait.For(async () => await window.GetIsLoaded());

        public static async Task<bool> GetIsLoaded(this IWindow window)
            => await window.GetProperty<bool>(nameof(Window.IsLoaded));
    }
}
