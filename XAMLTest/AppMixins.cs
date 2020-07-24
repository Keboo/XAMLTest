using System;
using System.Threading.Tasks;
using System.Windows;

namespace XamlTest
{
    public static class AppMixins
    {
        public static async Task InitializeWithDefaults(this IApp app, params string[] assemblies)
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

        public static async Task<IWindow> CreateWindowWithContent(this IApp app, string xamlContent,
            Size? windowSize = null,
            string title = "Test Window",
            WindowStartupLocation startupLocation = WindowStartupLocation.CenterScreen)
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
mc:Ignorable=""d""
Height=""{windowSize?.Height ?? 800}"" 
Width=""{windowSize?.Width ?? 1100}""
Title=""{title}""
WindowStartupLocation=""{startupLocation}"">
{xamlContent}
</Window>";

            return await app.CreateWindow(xaml);
        }
    }
}
