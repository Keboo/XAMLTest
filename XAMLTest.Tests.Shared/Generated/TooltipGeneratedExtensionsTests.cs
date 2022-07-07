namespace XamlTest.Tests.Generated;

#if WPF
partial class ToolTipGeneratedExtensionsTests
{
    static partial void OnClassInitialize()
    {
        GetWindowContent = x =>
        {
            return @$"
    <Window.ToolTip>
        {x}
    </Window.ToolTip>";
        };
    }
}
#endif