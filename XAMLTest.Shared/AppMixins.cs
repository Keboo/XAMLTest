﻿#if WPF
using System.Windows;
using System.Windows.Controls;
#endif

namespace XamlTest;

public static class AppMixins
{
    public static async Task InitializeWithDefaults(
        this IApp app,
        params string[] assemblies)
    {
        await InitializeWithResources(app, "", assemblies);
    }

    public static async Task InitializeWithResources(this IApp app, string resourceDictionaryContents, params string[] assemblies)
    {
        if (app is null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        await app.Initialize(@$"<ResourceDictionary xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml"">
{resourceDictionaryContents}
</ResourceDictionary>", assemblies);
    }

    public static async Task<IWindow> CreateWindowWithContent(
        this IApp app,
        string xamlContent,
        Size? windowSize = null,
        string title = "Test Window",
        string background = "White",
#if WPF
        WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen,
#endif
        string windowAttributes = "",
        params string[] additionalXmlNamespaces)
    {
        if (app is null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        string xaml = @$"<Window
xmlns=""http://schemas.microsoft.com/winfx/2006/xaml/presentation""
xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
xmlns:d=""http://schemas.microsoft.com/expression/blend/2008""
xmlns:mc=""http://schemas.openxmlformats.org/markup-compatibility/2006""
{string.Join(Environment.NewLine, additionalXmlNamespaces
.Select(x =>
{
    return x.StartsWith("xmlns") ? x : $"xmlns:{x}";
}))}
mc:Ignorable=""d""
";
#if WPF
        xaml += @$"
WindowStartupLocation=""{startupLocation}""
Background=""{background}""
Title=""{title}""
Height=""{windowSize?.Height ?? 800}"" 
Width=""{windowSize?.Width ?? 1100}""
";
#endif
        xaml += $@"
{windowAttributes}>
{xamlContent}
</Window>";

        IWindow window = await app.CreateWindow(xaml);
#if WIN_UI
        if (!string.IsNullOrEmpty(title))
        {
            await window.SetTitle(title);
        }
#endif
        return window;
    }

#if WPF
    public static async Task<IWindow> CreateWindowWithUserControl<TUserControl>(
        this IApp app,
        Size? windowSize = null,
        string title = "Test Window",
        string background = "White",
        WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen)
        where TUserControl : UserControl
    {
        if (app is null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return await app.CreateWindowWithContent($"<local:{typeof(TUserControl).Name} />",
            windowSize, title, background, startupLocation, additionalXmlNamespaces: @$"local=""clr-namespace:{typeof(TUserControl).Namespace};assembly={typeof(TUserControl).Assembly.GetName().Name}""");
    }
#elif WIN_UI
    public static async Task<IWindow> CreateWindowWithUserControl<TUserControl>(
        this IApp app,
        Size? windowSize = null,
        string title = "Test Window",
        string background = "White")
        where TUserControl : UserControl
    {
        if (app is null)
        {
            throw new ArgumentNullException(nameof(app));
        }

        return await app.CreateWindowWithContent($"<local:{typeof(TUserControl).Name} />",
            windowSize, title, background, additionalXmlNamespaces: @$"local=""clr-namespace:{typeof(TUserControl).Namespace};assembly={typeof(TUserControl).Assembly.GetName().Name}""");
    }
#endif
}
