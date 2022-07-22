// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

using System.Diagnostics;

namespace XamlTest.Tests.TestControls;

/// <summary>
/// An empty window that can be used on its own or navigated to within a Frame.
/// </summary>
public sealed partial class TestWindow : Window
{
    public TestWindow()
    {
        InitializeComponent2();
        Title = "Custom Test Window";
    }

    public void InitializeComponent2()
    {
        if (_contentLoaded)
            return;

        _contentLoaded = true;

        global::System.Uri resourceLocator = new global::System.Uri("ms-appx:///XAMLTest.WinUI3.Tests/TestControls/TestWindow.winui3.xaml");
        global::Microsoft.UI.Xaml.Application.LoadComponent(this, resourceLocator, global::Microsoft.UI.Xaml.Controls.Primitives.ComponentResourceLocation.Application);
    }
}
