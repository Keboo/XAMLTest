// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace XamlTest.Tests.TestControls;

public sealed partial class TextBlock_AttachedProperty : UserControl
{
    public static string GetMyCustomProperty(DependencyObject obj)
    {
        return (string)obj.GetValue(MyCustomPropertyProperty);
    }

    public static void SetMyCustomProperty(DependencyObject obj, string value)
    {
        obj.SetValue(MyCustomPropertyProperty, value);
    }

    public static readonly DependencyProperty MyCustomPropertyProperty =
        DependencyProperty.RegisterAttached("MyCustomProperty", typeof(string),
            typeof(TextBlock_AttachedProperty), new PropertyMetadata("Foo"));

    public TextBlock_AttachedProperty()
    {
        InitializeComponent();
    }
}
