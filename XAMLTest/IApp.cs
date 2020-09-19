﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace XamlTest
{
    public interface IApp : IAsyncDisposable, IDisposable
    {
        Task Initialize(string applicationResourceXaml, params string[] assemblies);
        Task<IWindow> CreateWindow(string xaml);
        Task<IWindow> CreateWindow<TWindow>() where TWindow : Window;
        Task<IWindow?> GetMainWindow();
        Task<IReadOnlyList<IWindow>> GetWindows();

        Task<IResource> GetResource(string key);
        Task<IImage> GetScreenshot();
    }
}
