#if WIN_UI
[assembly: GenerateHelpers(typeof(Microsoft.UI.Xaml.Window))]
[assembly: GenerateHelpers(typeof(Microsoft.UI.Xaml.Controls.Grid))]
[assembly: GenerateHelpers(typeof(Microsoft.UI.Xaml.Controls.StackPanel))]
[assembly: GenerateHelpers(typeof(Microsoft.UI.Xaml.Controls.TextBlock))]
[assembly: GenerateHelpers(typeof(Microsoft.UI.Xaml.Controls.TextBox))]
[assembly: GenerateHelpers(typeof(Microsoft.UI.Xaml.Controls.ListBoxItem))]
[assembly: GenerateHelpers(typeof(Microsoft.UI.Xaml.Controls.Primitives.FlyoutBase))]
#endif

namespace XamlTest;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
public class GenerateHelpersAttribute : Attribute
{
    public Type ControlType { get; set; }

    public string? Namespace { get; set; }

    public GenerateHelpersAttribute(Type controlType)
    {
        ControlType = controlType;
    }
}
