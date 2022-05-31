// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace XamlTest.Tests.TestControls;

public sealed partial class MouseClickPositions : UserControl
{
    public MouseClickPositions()
    {
        InitializeComponent();
    }

    private void UserControl_Tapped(object? sender, Microsoft.UI.Xaml.Input.TappedRoutedEventArgs e)
    {
        var p = e.GetPosition(this);
        ClickLocation.Text = $"{(int)p.X}x{(int)p.Y}";
    }
}
