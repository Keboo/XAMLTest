using System.Windows;
using System.Windows.Input;

namespace XamlTest.Tests.TestControls;

/// <summary>
/// Interaction logic for MouseClickPositions.xaml
/// </summary>
public partial class MouseClickPositions
{
    public MouseClickPositions()
    {
        InitializeComponent();
    }

    private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        Point p = e.GetPosition(this);
        ClickLocation.Text = $"{(int)p.X}x{(int)p.Y}";
    }
}
