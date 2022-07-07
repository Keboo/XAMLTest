namespace XamlTest.Tests.TestControls;

/// <summary>
/// Interaction logic for TextBlock_AttachedProperty.xaml
/// </summary>
public partial class TextBlock_AttachedProperty : UserControl
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
