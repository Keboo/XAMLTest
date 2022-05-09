using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows;

namespace XamlTest.Tests.Simulators;

public class App : IApp
{
    public Task<IWindow> CreateWindow(string xaml)
    {
        throw new NotImplementedException();
    }

    public Task<IWindow> CreateWindow<TWindow>() where TWindow : Window
    {
        throw new NotImplementedException();
    }

    public void Dispose()
    { }

    private static ValueTask Completed { get; } = new();
    public ValueTask DisposeAsync() => Completed;

    public Task<IWindow?> GetMainWindow()
    {
        throw new NotImplementedException();
    }

    public Task<IResource> GetResource(string key)
    {
        throw new NotImplementedException();
    }

    public Task<IImage> GetScreenshot()
        => Task.FromResult<IImage>(new Image());
    public Task<IReadOnlyList<ISerializer>> GetSerializers() => throw new NotImplementedException();

    public Task<IReadOnlyList<IWindow>> GetWindows()
    {
        throw new NotImplementedException();
    }

    public Task Initialize(string applicationResourceXaml, params string[] assemblies)
    {
        throw new NotImplementedException();
    }

    public Task RegisterSerializer<T>(int insertIndex = 0) where T : ISerializer, new() => throw new NotImplementedException();
}
